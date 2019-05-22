using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace phirSOFT.Collections
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides a <see cref="T:System.Collections.Generic.IDictionary`2" /> that will create missing values when requested,
    ///     but will not keep a strong reference to it.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    public class WeakLazyDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TValue : class
    {
        private readonly Dictionary<TKey, WeakReference<TValue>> _dictionary =
            new Dictionary<TKey, WeakReference<TValue>>();

        private readonly Func<TKey, TValue> _generatorFunc;

        public WeakLazyDictionary(Func<TKey, TValue> generatorFunc)
        {
            _generatorFunc = generatorFunc;
        }


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.Select(kv =>
                {
                    var exists = kv.Value.TryGetTarget(out var value);
                    return new {kv.Key, exists, value};
                })
                .Where(p => p.exists)
                .Select(p => new KeyValuePair<TKey, TValue>(p.Key, p.value))
                .GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item) && _dictionary.Remove(item.Key);
        }

        public int Count => _dictionary.Count;

        public bool IsReadOnly => true;

        public void Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_dictionary.ContainsKey(key))
            {
                _dictionary[key].TryGetTarget(out value);
                return true;
            }
            value = null;
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dictionary.TryGetValue(key, out var reference) &&
                    reference.TryGetTarget(out var value))
                    return value;
                value = _generatorFunc(key);
                _dictionary[key] = new WeakReference<TValue>(value);
                return value;
            }
            set => throw new NotSupportedException();
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values
            .Select(reference =>
            {
                var test = reference.TryGetTarget(out var value);
                return new {test, value};
            })
            .Where(t => t.test)
            .Select(t => t.value)
            .ToList();
    }
}