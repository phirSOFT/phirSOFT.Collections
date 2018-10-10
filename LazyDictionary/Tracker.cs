using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace phirSOFT.LazyDictionary
{
    /// <summary>
    /// Track cycles in forward linked lists.
    /// </summary>
    /// <remarks>
    /// This class shouldn't be used public.
    /// <para/>
    ///
    /// The class uses the following apporach:
    /// If a single linked list has a cycle there is a 'n' so that start + n == start + 2n;
    /// </remarks>
    internal sealed unsafe class Tracker : IDisposable
    {
        private TrackerNode* _head;
        private TrackerNode* _tail;
        private bool _oddStep;

        /// <summary>
        /// Gets, whether there is currently a detected cycle. This property won't memorize a previous found.
        /// </summary>
        public bool FoundCircle => !_oddStep && _head->Value == _tail->Value;

        /// <summary>
        ///     Adds a step to the tracer.
        /// </summary>
        /// <param name="value">
        ///     A pointer to the value of the step.
        /// </param>
        /// <remarks>
        /// <paramref name="value"/> is never dereferenced. Two values are considered equal iff the pointers are equal.
        /// </remarks>
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

        /// <summary>
        /// Stored a node of the single linked list.
        /// </summary>
        private struct TrackerNode
        {
            /// <summary>
            /// The value of the node
            /// </summary>
            public void* Value;

            /// <summary>
            /// A pointer to the next node.
            /// </summary>
            public TrackerNode* Next;
        }

        /// <summary>
        /// Disposes all existing nodes
        /// </summary>
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
