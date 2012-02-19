using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MoviesBroker.Caching
{
    public class UrlRequestCache<T>
    {
        private static readonly ConcurrentDictionary<string, Task<T>> Cache = new ConcurrentDictionary<string, Task<T>>();

        private static readonly UrlRequestCache<T> _instance;

        static UrlRequestCache()
        {
            _instance = new UrlRequestCache<T>();
        }

        private UrlRequestCache()
        {
        }

        public static UrlRequestCache<T> Instance
        {
            get
            {
                return _instance;
            }
        }

        public bool Contains(string url)
        {
            return Cache.ContainsKey(url);
        }

        public void Add(string url, Task<T> action)
        {
            if (!Cache.ContainsKey(url))
            {
                Cache[url] = new Task<T>(() => action.Result);
            }
        }

        public Task<T> Get(string url)
        {
            return Cache[url];
        }
    }
}