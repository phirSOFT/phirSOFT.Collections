using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace phirSOFT.LazyDictionary
{
    public partial class LazyCache<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, int> _cache;
        private readonly Func<TKey, TValue> _valueFactory;
        private readonly CacheItem[] _cacheItems;
        private readonly int _capacity;
        private readonly int _headCapacity;
        public LazyCache(Func<TKey, TValue> valueFactory) : this(valueFactory, 64)
        {

        }

        public LazyCache(Func<TKey, TValue> valueFactory, int minimumCapacity)
        {
            _valueFactory = valueFactory;
            (_capacity, _headCapacity) = ComputeCapacity(minimumCapacity);
            _cacheItems = new CacheItem[_capacity];
            _cache = new Dictionary<TKey, int>();
        }

        public int Count => _cache.Count;

        public IEnumerable<TKey> Keys
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return _cacheItems[i].Key;
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return _cacheItems[i].Value;
                }
            }
        }
        public TValue this[TKey key] => GetValue(key);

        private TValue GetValue(TKey key)
        {
            if (!_cache.TryGetValue(key, out var cacheIndex))
            {
                var cacheItem = new CacheItem
                {
                    Value = _valueFactory(key)
                };

                cacheIndex = Count < _capacity ? Count : _capacity;
                _cache.Remove(_cacheItems[cacheIndex].Key);

                _cache.Add(key, cacheIndex);
                _cacheItems[cacheIndex] = cacheItem;
                return cacheItem.Value;
            }
            else
            {
                ulong hits = ++_cacheItems[cacheIndex].Hits;
                bool predecessorInHead = cacheIndex <= _headCapacity;
                int swapItem = predecessorInHead ? cacheIndex - 1 : _headCapacity - 1;

                // Shall we transpose the items or move the item into head
                if (cacheIndex > 0 && hits > _cacheItems[swapItem].Hits)
                {
                    SwapItems(cacheIndex, swapItem);
                    cacheIndex = swapItem;
                }
                else if (!predecessorInHead)
                {
                    SwapItems(cacheIndex, cacheIndex - 1);
                    cacheIndex--;
                }
            }

            return _cacheItems[cacheIndex].Value;
        }


        private void SwapItems(int a, int b)
        {
            CacheItem aItem = _cacheItems[a];
            CacheItem bItem = _cacheItems[b];
            _cache[aItem.Key] = b;
            _cache[bItem.Key] = a;
            _cacheItems[a] = bItem;
            _cacheItems[b] = aItem;
        }

        private static (int capacity, int headCapacity) ComputeCapacity(int x)
        {
            if ((x & (x - 1)) != 0)
            {
                x = x << 1;
            }

            int capacity = 1;
            int headCapacity = 1;
            while ((x = x >> 2) != 0)
            {
                headCapacity = headCapacity << 1;
                capacity = capacity << 2;
            }
            return (capacity, headCapacity);
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = GetValue(key);
            return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                var item = _cacheItems[i];
                yield return new KeyValuePair<TKey, TValue>(item.Key, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
