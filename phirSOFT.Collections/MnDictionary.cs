using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace phirSOFT.Collections
{
    /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
    /// <summary>
    ///     Provides a dictionary that can have multiple values associated with one key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the values</typeparam>
    public class MnDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, IEnumerable<TValue>>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _store = new Dictionary<TKey, HashSet<TValue>>();
        private readonly Dictionary<TValue, HashSet<TKey>> _inverseStore = new Dictionary<TValue, HashSet<TKey>>();


        public MnDictionary()
        {
            Inverse = (IReadOnlyDictionary<TValue, IEnumerable<TKey>>) new ReadOnlyDictionary<TValue, HashSet<TKey>>(_inverseStore);
        }

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
            _inverseStore.Clear();
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
            if (!_store.TryGetValue(item.Key, out var list) || !list.Remove(item.Value))
                return false;

            _inverseStore[item.Value].Remove(item.Key);

            if (list.Count == 0)
                _store.Remove(item.Key);

            if (_inverseStore[item.Value].Count == 0)
                _inverseStore.Remove(item.Value);

            Count--;
            return true;
        }

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            Add(_store, key, value);
            Add(_inverseStore, value, key);

            Count++;
        }

        public bool ContainsKey(TKey key)
        {
            return _store.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _inverseStore.ContainsKey(value);
        }

        public bool Remove(TKey key)
        {
            var (sucess, numRemoved) = Remove(_store, _inverseStore, key);
            Count -= numRemoved;
            return sucess;
        }

        public bool RemoveValue(TValue value)
        {
            var (sucess, numRemoved) = Remove(_inverseStore, _store, value);
            Count -= numRemoved;
            return sucess;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var result = _store.TryGetValue(key, out var list);
            value = result && list.Count == 1 ? list.First() : default;
            return result;
        }

        public TValue this[TKey key]
        {
            get => TryGetValue(key, out TValue value) ? value : throw new NotSupportedException("Indexer is not permitted for multi value keys");
            set => Add(key, value);
        }

        public ICollection<TKey> Keys => _store.Keys;
        public ICollection<TValue> Values => _inverseStore.Keys;

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

        public IReadOnlyDictionary<TValue, IEnumerable<TKey>> Inverse { get; }

        private static void Add<TKey, TValue>(IDictionary<TKey, HashSet<TValue>> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryGetValue(key, out HashSet<TValue> values))
            {
                values = new HashSet<TValue>();
                dictionary.Add(key, values);
            }

            values.Add(value);
        }

        private static (bool sucess, int removed) Remove<TKey, TValue>(IDictionary<TKey, HashSet<TValue>> dictionary, IDictionary<TValue, HashSet<TKey>> inverseDictionary, TKey key)
        {
            if (!dictionary.TryGetValue(key, out var list))
                return (false, 0);

            dictionary.Remove(key);
            
            foreach (var value in list)
            {
                var inverse = inverseDictionary[value];
                inverse.Remove(key);
                if (inverse.Count == 0)
                    inverseDictionary.Remove(value);
            }

            return (true, list.Count);
        }

    }
}