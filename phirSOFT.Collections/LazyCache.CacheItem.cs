using System;
using System.Collections.Generic;
using System.Text;

namespace phirSOFT.Collections
{
    public partial class LazyCache<TKey, TValue>
    {
        private struct CacheItem
        {
            public TValue Value;
            public TKey Key;
            public ulong Hits;
        }
    }
}
