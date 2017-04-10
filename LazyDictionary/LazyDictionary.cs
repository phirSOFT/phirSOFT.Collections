using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;

namespace phirSOFT
{
    /// <summary>
    /// Provides a <see cref="IDictionary{TKey,TValue}"/> that will create missing values when requested.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    public class LazyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;
        private readonly Func<TKey, TValue> _generatorFunc;

        /// <summary>
        /// Creates a new Lazy dictionary with an genearator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate miisng members</param>
        public LazyDictionary(Func<TKey, TValue> generatorFunc) : this(generatorFunc, new Dictionary<TKey, TValue>())
        { }

        /// <summary>
        /// Creates a new Lazy dictionary with an genearator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate miisng members</param>
        /// <param name="comparer">The comparer that should be used for comparing the keys.</param>
        public LazyDictionary(Func<TKey,TValue> generatorFunc, IEqualityComparer<TKey> comparer) : this(generatorFunc, new Dictionary<TKey, TValue>(comparer))
        { }

        /// <summary>
        /// Creates a new Lazy dictionary with an genearator function.
        /// </summary>
        /// <param name="generatorFunc">The function used to generate miisng members</param>
        /// <param name="dictionary">The dictionary that should be used for storing.</param>
        /// <remarks>The dictionary can be prefilled. But must not be readonly. If its readonly an exception will be thrown.</remarks>
        public LazyDictionary(Func<TKey, TValue> generatorFunc, IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if(dictionary.IsReadOnly)
                throw new ArgumentException("Invalid Dictionary. The dictionary must not be readonly", nameof(dictionary));

            _generatorFunc = generatorFunc ?? throw new ArgumentNullException(nameof(generatorFunc));
            _dictionary = dictionary;
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dictionary).GetEnumerator();
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        /// <remarks>This value determs, wheter the value is allready in the dictionary. This will not invoke the geenrator</remarks>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Remove(item);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public int Count => _dictionary.Count;

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public bool IsReadOnly => _dictionary.IsReadOnly;

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
        }


        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_dictionary.TryGetValue(key, out value))
            {
                value = GenerateEntry(key);
            }
            return true;
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public TValue this[TKey key]
        {
            get
            {
                TryGetValue(key, out var retval);
                return retval;
            }
            set { _dictionary[key] = value; }
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public ICollection<TKey> Keys
        {
            get { return _dictionary.Keys; }
        }

        /// <inheritdoc cref="IDictionary{TKey,TValue}"/>
        public ICollection<TValue> Values
        {
            get { return _dictionary.Values; }
        }

        private TValue GenerateEntry(TKey key)
        {
            var value = _generatorFunc(key);
            Add(new KeyValuePair<TKey, TValue>(key, value));
            return value;
        }
    }
}
