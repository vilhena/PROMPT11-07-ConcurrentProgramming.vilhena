using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PROMPThreadPool
{
    public delegate void Proc();

    public class ThreadPool
    {
        private readonly int _min = 0;
        private readonly int _max = 0;
        private int _threadCount = 0;
        private int _doingWork = 0;
        private readonly Queue<Proc> _items = new Queue<Proc>();


        public ThreadPool(int min, int max)
        {
            _min = min;
            _max = max;
            _threadCount = _min;

            for (int i = 0; i < _min; i++)
            {
                var newT = new Thread(DoWork);
                newT.Start();
            }
        }

        private void DoWork()
        {

            Proc res;
            bool first = true;

            while (true)
            {
                lock (_items)
                {
                    if (!first)
                    {
                        _doingWork--;
                        if (_threadCount > _min && _items.Count == 0 && _doingWork != (_threadCount - 1))
                        {
                            _threadCount--;
                            return;
                        }
                    }
                    first = false;

                    while (_items.Count == 0) Monitor.Wait(_items);
                    
                    _doingWork++;
                    if (_threadCount < _max && _doingWork == _threadCount)
                    {
                        var newT = new Thread(DoWork);
                        _threadCount++;
                        newT.Start();
                    }

                    res = _items.Dequeue();
                }

                res.Invoke();

            }
        }

        public void QueueProc(Proc proc)
        {
            lock (_items)
            {
                _items.Enqueue(proc);
                Monitor.Pulse(_items);
            }
        }
    }
}
