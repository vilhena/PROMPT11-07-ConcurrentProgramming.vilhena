using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SyncronizedCacheApp
{
    class UnboundedQueue<T>
    {
        private readonly LinkedList<T> _item = new LinkedList<T>();

        public T Get()
        {
            lock (_item)
            {
                while (_item.Count == 0)
                {
                    try
                    {
                        Monitor.Wait(this);
                    }
                    catch(ThreadInterruptedException)
                    {
                        if (_item.Count > 0) // regeneracao de notificacao
                            Monitor.Pulse(this);

                        throw;
                    }
                }
                var res = _item.First;
                _item.Remove(res);
                return res.Value; 
            }
        }

        public void Put(T value)
        {
            lock (_item)
            {
                _item.AddLast(value);
                Monitor.Pulse(_item);
            }
        }
    }
}
