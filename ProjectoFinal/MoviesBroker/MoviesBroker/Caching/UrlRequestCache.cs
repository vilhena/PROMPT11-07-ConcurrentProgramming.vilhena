using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MoviesBroker.Caching
{
    //Generic url request task cache, because its generic, we will have one cache per each generic Type
    public class UrlRequestCache<T>
    {
        private static readonly ConcurrentDictionary<string, Task<T>> Cache = new ConcurrentDictionary<string, Task<T>>();
        private static readonly UrlRequestCache<T> _instance;

        //Singleton ThreadSafe Implementation
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

        public Task<T> TryAdd(string url, Task<T> action)
        {
            if (!Cache.ContainsKey(url))
            {
                var tcs = new TaskCompletionSource<T>();
                var valueTask = new Task(() =>
                                             {
                                                 //needs task error try catch
                                                 var actionResult = action.Result;
                                                 tcs.SetResult(actionResult);
                                             });
                var added = Cache.TryAdd(url,tcs.Task); //if fails someone allready added
                //this should be addressed with lazy load (only when i use the task.Result
                if (added) //only runs if the task was added otherwise someone already added and it will on the cache running
                    valueTask.Start(); //Schedules the action result read
            }
            return Cache[url];
        }


        internal Task<T> Get(string url)
        {
            return Cache[url];
        }
    }
}