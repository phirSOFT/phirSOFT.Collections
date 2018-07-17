using System;
using System.Collections;
using System.Collections.Generic;

namespace phirSOFT.LazyDictionary
{
    /// <inheritdoc />
    /// <summary>
    ///     Provides a <see cref="T:System.Collections.Generic.IDictionary`2" /> that will create missing values when requested.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    public class LazyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly Func<TKey, TValue> _generatorFunc;


        /// <inheritdoc />
        /// <summary>
        ///     Creates a new Lazy dictionary with a generator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate missing members.</param>
        public LazyDictionary(Func<TKey, TValue> generatorFunc) : this(generatorFunc, new Dictionary<TKey, TValue>())
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creates a new Lazy dictionary with a generator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate missing members.</param>
        /// <param name="comparer">The comparer that will be used to compare the keys.</param>
        public LazyDictionary(Func<TKey, TValue> generatorFunc, IEqualityComparer<TKey> comparer) : this(generatorFunc,
            new Dictionary<TKey, TValue>(comparer))
        {
        }

        /// <summary>
        ///     Creates a new Lazy dictionary with a generator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate missing members</param>
        /// <param name="dictionary">The dictionary that will be used for storing.</param>
        /// <remarks>The dictionary can be prefilled, but must not be readonly or an exception will be thrown.</remarks>
        public LazyDictionary(Func<TKey, TValue> generatorFunc, IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (dictionary.IsReadOnly)
                throw new ArgumentException("Invalid Dictionary. The dictionary must not be readonly",
                    nameof(dictionary));

            _generatorFunc = generatorFunc ?? throw new ArgumentNullException(nameof(generatorFunc));
            _dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictionary).GetEnumerator();
        }

        
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
        }

        
        public void Clear()
        {
            _dictionary.Clear();
        }

        
        /// <inheritdoc />
        /// <remarks>This determines whether the value is already in the dictionary. This will not invoke the generator.</remarks>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)       
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item);
        }

        
        public int Count => _dictionary.Count;

        
        public bool IsReadOnly => _dictionary.IsReadOnly;

        
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
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
            if (!_dictionary.TryGetValue(key, out value))
                value = GenerateEntry(key);
            return true;
        }

        
        public TValue this[TKey key]
        {
            get
            {
                TryGetValue(key, out var retval);
                return retval;
            }
            set => _dictionary[key] = value;
        }

        
        public ICollection<TKey> Keys => _dictionary.Keys;

        
        public ICollection<TValue> Values => _dictionary.Values;

        private TValue GenerateEntry(TKey key)
        {
            var value = _generatorFunc(key);
            Add(new KeyValuePair<TKey, TValue>(key, value));
            return value;
        }
    }
}
