using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace phirSOFT.LazyDictionary
{
    /// <summary>
    /// Track cycles in forward linked lists
    /// </summary>
    internal sealed unsafe class Tracker : IDisposable
    {
        private TrackerNode* _head;
        private TrackerNode* _tail;
        private bool _oddStep;

        public bool FoundCircle => !_oddStep && _head->Value == _tail->Value;

        public void AddStep(void* value)
        {
            _head = _head->Next = (TrackerNode*) Marshal.AllocHGlobal(sizeof(TrackerNode));
            _head->Value = value;

            // ReSharper disable once AssignmentInConditionalExpression
            if (_oddStep = !_oddStep) return;

            var tail = (IntPtr) _tail;
            _tail = _tail->Next;
            Marshal.FreeHGlobal( tail);
        }

        private struct TrackerNode
        {
            public void* Value;
            public TrackerNode* Next;
        }

        public void Dispose()
        {
            while (_head != null)
            {
                var headPtr = (IntPtr) _head;
                _head = _head->Next;
                Marshal.FreeHGlobal(headPtr);
            }
        }
    }
}
