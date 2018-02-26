using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace phirSOFT.LazyDictionary
{
    /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
    /// <summary>
    ///     Provides a dictionary that can have multiple values associated with one key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the values</typeparam>
    public class MnDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, IEnumerable<TValue>>
    {
        private readonly Dictionary<TKey, LinkedList<TValue>> _store = new Dictionary<TKey, LinkedList<TValue>>();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _store.SelectMany(k => k.Value.Select(v => new KeyValuePair<TKey, TValue>(k.Key, v)))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _store.Clear();
            Count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _store.TryGetValue(item.Key, out var list) && list.Contains(item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            using (var pairs = GetEnumerator())
            {
                while (arrayIndex < array.Length && pairs.MoveNext()) array[arrayIndex++] = pairs.Current;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_store.TryGetValue(item.Key, out var list) || !list.Remove(item.Value)) return false;
            if (list.Count == 0)
                _store.Remove(item.Key);
            Count--;
            return true;
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            if (!_store.ContainsKey(key))
                _store.Add(key, new LinkedList<TValue>());
            _store[key].AddLast(value);
            Count++;
        }

        public bool ContainsKey(TKey key)
        {
            return _store.ContainsKey(key);
        }


        public bool Remove(TKey key)
        {
            if (!_store.TryGetValue(key, out var list)) return false;
            Count -= list.Count;
            _store.Remove(key);
            return true;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = _store.TryGetValue(key, out var list);
            value = result ? list.First.Value : default(TValue);
            return result;
        }

        public TValue this[TKey key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public ICollection<TKey> Keys => _store.Keys;
        public ICollection<TValue> Values => _store.Values.SelectMany(c => c).ToList().AsReadOnly();

        IEnumerator<KeyValuePair<TKey, IEnumerable<TValue>>> IEnumerable<KeyValuePair<TKey, IEnumerable<TValue>>>.
            GetEnumerator()
        {
            return _store.Select(kv => new KeyValuePair<TKey, IEnumerable<TValue>>(kv.Key, kv.Value)).GetEnumerator();
        }

        public bool TryGetValue(TKey key, out IEnumerable<TValue> value)
        {
            var result = _store.TryGetValue(key, out var list);
            value = list;
            return result;
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, IEnumerable<TValue>>.this[TKey key] => _store[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, IEnumerable<TValue>>.Keys => Keys;

        IEnumerable<IEnumerable<TValue>> IReadOnlyDictionary<TKey, IEnumerable<TValue>>.Values => _store.Values;
    }
}