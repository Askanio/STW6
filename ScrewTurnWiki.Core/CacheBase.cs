using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;

namespace ScrewTurn.Wiki
{

    /// <summary>
    /// 
    /// </summary>
    public delegate object GetData();

    internal class CacheBase
    {
        private static readonly ObjectCache Cache = MemoryCache.Default;
		private readonly DateTimeOffset _expirationDateTime;

		public CacheBase(TimeSpan expirationTime)
		{
            _expirationDateTime = DateTimeOffset.Now.Add(expirationTime);
		}

		public Object GetCachedItem(String cacheKeyPrefix, String cacheKeyName, GetData dataMetod)
		{
			return GetCachedItem(String.Concat(cacheKeyPrefix, cacheKeyName), dataMetod);
		}

		public Object GetCachedItem(String cacheKeyName, GetData dataMetod)
		{
			var value = GetCachedItem(cacheKeyName);
			if (value == null)
			{
				value = dataMetod();
				if (value != null)
				    Cache.Set(cacheKeyName, value, _expirationDateTime);
			}
			return value;
		}

		public void AddToCache(String cacheKeyPrefix, String cacheKeyName, Object cacheItem)
		{
			AddToCache(String.Concat(cacheKeyPrefix, cacheKeyName), cacheItem);
		}

        public void AddToCache(String cacheKeyName, Object cacheItem)
        {
			Cache.Set(cacheKeyName, cacheItem, _expirationDateTime);
        }

        public Object GetCachedItem(String cacheKeyName)
        {
            return Cache[cacheKeyName] as Object;
        }

        public void RemoveCachedItem(String cacheKeyName)
        {
            if (Cache.Contains(cacheKeyName))
                Cache.Remove(cacheKeyName);
        }

        public void RemoveCachedItem(String cacheKeyPrefix, String cacheKeyName)
        {
            RemoveCachedItem(String.Concat(cacheKeyPrefix, cacheKeyName));
        }

        public void RemoveCachedItems(String endCacheKeyName)
        {
            var forRemove = Cache.Where(t => t.Key.EndsWith(endCacheKeyName)).ToList();
            foreach (var b in forRemove)
                Cache.Remove(b.Key);
        }

        //public void RemoveAllCachedItem(String cacheKeyPrefix)
        //{

        //    Cache.Select(t => t.Key.StartsWith(cacheKeyPrefix)).
        //        Cache.Remove(cacheKeyName);
        //}
    }
}

