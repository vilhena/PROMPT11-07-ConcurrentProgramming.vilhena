using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SyncronizedCacheApp
{
    public class ConcurrentCache<TKey, TValue>
    {
        public class ValueFactory
        {
            private TValue _result = default(TValue);
            private volatile bool _ready = false;

            private readonly object _o = new object();

            
            public TValue Result
            {
                get
                {
                    if (!_ready)
                    {
                        lock (_o)
                        {
                            if (_ready)
                                return _result;

                            Monitor.Wait(_o);
                        }
                    }

                    return _result;
                }
                set
                {
                    lock (_o)
                    {
                        var result = value;
                        _ready = true;
                        Monitor.PulseAll(_o);
                    }
                }
            }

        }

        private readonly Dictionary<TKey, ValueFactory> _cache = new Dictionary<TKey, ValueFactory>();
        private readonly Func<TKey, TValue> _factory;

        // construtor recebe o factory que sabe criar o objecto com a chave indicada
        public ConcurrentCache(Func<TKey, TValue> factory)
        {
            _factory = factory;
        }

        // indexer que permita o acesso ao valor associado a chave indicada (a definir)
        // ----

        public ValueFactory this[TKey key]
        {
            get {
                if (_cache.ContainsKey(key))
                {
                    return _cache[key];
                }

                ValueFactory future;

                lock (this)
                {
                    if (_cache.ContainsKey(key))
                    {
                        return _cache[key];
                    }

                    future = new ValueFactory();
                    _cache[key] = future;
                }

                ThreadPool.QueueUserWorkItem((o) =>
                {
                    future.Result = _factory(key);
                });

                return future;
            }
            
        }


    }

}
