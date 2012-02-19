using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

namespace LockFree
{
    public class _SpinLock
    {
#pragma warning disable 0420

        private const int FREE = 0;
        private const int BUSY = 1;
        private volatile int state;

        public void Enter()
        {
            SpinWait sw = new SpinWait();
            do
            {
                if (state == FREE && Interlocked.Exchange(ref state, BUSY) == FREE)
                {
                    return;
                }
                do
                {
                    sw.SpinOnce();
                } while (state == BUSY);
            } while (true);
        }

        public void Exit()
        {
            state = FREE;
        }
#pragma warning restore 0420
    }

    public class ParkSpot
    {
        //...
        [ThreadStatic]
        private static EventWaitHandle _threadLocalEvent;

        //...
        private volatile bool isWaiting;
        private volatile bool alreadySet;

        //...
        private volatile EventWaitHandle _waitEvent;

        //...

        private static EventWaitHandle WaitEvent
        {
            get
            {
                EventWaitHandle wev;
                if ((wev = _threadLocalEvent) == null)
                {
                    wev = _threadLocalEvent = new AutoResetEvent(false);
                }
                return wev;
            }
        }

        public void Park()
        {

            if (alreadySet)
            {
                return;
            }
            _waitEvent = WaitEvent;
            isWaiting = true;
            Thread.MemoryBarrier();
            if (!alreadySet)
            {
                _waitEvent.WaitOne();
            }
        }

        public void Unpark()
        {
            alreadySet = true;
            Thread.MemoryBarrier();
            if (isWaiting)
            {
                _waitEvent.Set();
            }
        }
    }

    public class _Semaphore
    {
        private int count;

        // wait queue
        private Queue<ParkSpot> waitQueue = new Queue<ParkSpot>();

        public _Semaphore(int initial)
        {
            if (initial > 0)
            {
                count = initial;
            }
        }

        public void Down()
        {
            ParkSpot waiter = null;
            lock (waitQueue)
            {
                if (count > 0)
                {
                    count--;
                    return;
                }
                waitQueue.Enqueue(waiter = new ParkSpot());
            }
            waiter.Park();
        }

        public void Up()
        {
            ParkSpot waiter = null;
            lock (waitQueue)
            {
                if (waitQueue.Count == 0)
                {
                    count++;
                    return;
                }
                waiter = waitQueue.Dequeue();
            }
            waiter.Unpark();
        }
    }

    public class _ConcurrentStack<T>
    {

#pragma warning disable 0420

        private class Node<V>
        {
            internal Node<V> next;
            internal V value;

            internal Node(V v)
            {
                value = v;
            }
        }

        private volatile Node<T> top;

        // Implemented...

        public void Push(T value)
        {
            Node<T> nn = new Node<T>(value);
            SpinWait sw = new SpinWait();
            do
            {
                Node<T> t = top;
                nn.next = t;
                if (Interlocked.CompareExchange(ref top, nn, t) == t)
                {
                    return;
                }
                sw.SpinOnce();
            } while (true);
        }

        public bool TryPop(out T value)
        {
#error TO BE IMPLEMENTED
            value = default(T);
            return false;
        }

        // ...

        public T Pop()
        {
            SpinWait sw = new SpinWait();
            T v;
            while (!TryPop(out v))
            {
                sw.SpinOnce();
            }
            return v;
        }
#pragma warning restore 0420
    }

    //
    // Concurrent Queue.
    //

    internal class _ConcurrentQueue<T>
    {

#pragma warning disable 0420

        //
        // The queue node.
        //

        private class Node<V>
        {
            internal Node<V> next;
            internal V value;

            internal Node(V v)
            {
                value = v;
            }

            internal Node()
            {
                value = default(V);
            }
        }

        //
        // The head and the tail of the concurrent queue.
        //

        private volatile Node<T> head;
        private volatile Node<T> tail;

        //
        // Constructor.
        //

        internal _ConcurrentQueue()
        {
            head = tail = new Node<T>();
        }

        //
        // Advances the queue's head.
        //

        private bool AdvanceHead(Node<T> h, Node<T> nh)
        {
            if (head == h && Interlocked.CompareExchange<Node<T>>(ref head, nh, h) == h)
            {
                h.next = h;     // Forget next.
                return true;
            }
            return false;
        }

        //
        // Advances the queue's tail.
        //

        private bool AdvanceTail(Node<T> t, Node<T> nt)
        {
            return (tail == t && Interlocked.CompareExchange<Node<T>>(ref tail, nt, t) == t);
        }

        //
        // Enqueues a node with the specified value.
        //

        internal void Enqueue(T value)
        {
            Node<T> nn = new Node<T>(value);
            do
            {
                Node<T> t = tail;
                Node<T> tn = t.next;

                //
                // If the queue is in the intermediate state, try to advance its tail.
                //

                if (tn != null)
                {
                    AdvanceTail(t, tn);
                    continue;
                }

                //
                // Queue in quiescent state, so try to insert the new node.
                //

                if (Interlocked.CompareExchange<Node<T>>(ref t.next, nn, null) == null)
                {

                    //
                    // Advance the tail and return.
                    //

                    AdvanceTail(t, nn);
                    return;
                }
            } while (true);
        }

        //
        // Tries to dequeue the next node from queue.
        //

        internal bool TryDequeue(out T value)
        {
#error TO BE IMPLEMENTED
            value = default(T);
            return false;
        }

        internal T Dequeue()
        {
            SpinWait sw = new SpinWait();
            T v;
            while (!TryDequeue(out v))
            {
                sw.SpinOnce();
            }
            return v;
        }

#pragma warning restore 0420
    }

    class SemaphoreTest
    {
        private const int THREADS = 100;

        private volatile bool running;

        private _Semaphore s = new _Semaphore(1);

        private void DownUp(object tidx)
        {
            int tid = (int)tidx;
            Random r = new Random(tid);
            long count = 0;
            do
            {
                Thread.Sleep(r.Next(5));
                s.Down();
                //Thread.Sleep(r.Next(1));
                //Thread.SpinWait(1000);
                s.Up();
                if ((++count % 100) == 0)
                {
                    Console.Write("[{0}]", tid);
                }
            } while (running);
            Console.Write("{0}-- thread [{1}] exiting after {2} Down/Up", Environment.NewLine, tid, count);
        }

        public void Run()
        {
            running = true;
            for (int i = 0; i < THREADS; i++)
            {
                new Thread(DownUp).Start(i + 1);
            }
        }

        public void Stop()
        {
            running = false;
        }
    }

    class SpinLockTest
    {
        private const int THREADS = 50;

        private volatile bool running;

        private _SpinLock sl = new _SpinLock();

        private void EnterExit(object tidx)
        {
            int tid = (int)tidx;
            Random r = new Random(tid);
            long count = 0;
            do
            {
                Thread.SpinWait(r.Next(1000000));
                sl.Enter();
                Thread.SpinWait(r.Next(1000));
                sl.Exit();
                if ((++count % 100) == 0)
                {
                    Console.Write("[{0}]", tid);
                }
            } while (running);
            Console.Write("{0}-- thread [{1}] exiting after {2} Enter/Exit", Environment.NewLine, tid, count);
        }

        public void Run()
        {
            running = true;
            for (int i = 0; i < THREADS; i++)
            {
                Thread ee = new Thread(EnterExit);
                ee.Priority = ThreadPriority.BelowNormal;
                ee.Start(i + 1);
            }
        }

        public void Stop()
        {
            running = false;
        }
    }

    class ConcurrentStackTest
    {
        private const int THREADS = 50;

        private volatile bool running;

        private _ConcurrentStack<int> cs = new _ConcurrentStack<int>();

        private void PopPush(object tidx)
        {
            int tid = (int)tidx;
            Random r = new Random(tid);
            long count = 0;
            do
            {
                Thread.Sleep(r.Next(10));
                int v = cs.Pop();
                Debug.Assert(v == 42);
                Thread.Sleep(r.Next(1));
                cs.Push(v);
                if ((++count % 100) == 0)
                {
                    Console.Write("[{0}]", tid);
                }
            } while (running);
            Console.Write("{0}-- thread [{1}] exiting after {2} Pop/Push", Environment.NewLine, tid, count);
        }

        public void Run()
        {
            running = true;
            cs.Push(42);
            for (int i = 0; i < THREADS; i++)
            {
                Thread pp = new Thread(PopPush);
                pp.Priority = ThreadPriority.BelowNormal;
                pp.Start(i + 1);
            }
        }

        public void Stop()
        {
            running = false;
        }
    }

    class ConcurrentQueueTest
    {
        private const int THREADS = 50;

        private volatile bool running;

        private _ConcurrentQueue<int> cq = new _ConcurrentQueue<int>();

        private void DequeueEnqueue(object tidx)
        {
            int tid = (int)tidx;
            Random r = new Random(tid);
            long count = 0;
            do
            {
                Thread.Sleep(r.Next(10));
                int v = cq.Dequeue();
                Debug.Assert(v == 42);
                Thread.Sleep(r.Next(1));
                cq.Enqueue(v);
                if ((++count % 100) == 0)
                {
                    Console.Write("[{0}]", tid);
                }
            } while (running);
            Console.Write("{0}-- thread [{1}] exiting after {2} Dequeue/Enqueue", Environment.NewLine, tid, count);
        }

        public void Run()
        {
            running = true;
            cq.Enqueue(42);
            for (int i = 0; i < THREADS; i++)
            {
                Thread de = new Thread(DequeueEnqueue);
                de.Priority = ThreadPriority.BelowNormal;
                de.Start(i + 1);
            }
        }

        public void Stop()
        {
            running = false;
        }
    }

    class LockFree
    {
        static void Main()
        {
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            SemaphoreTest test = new SemaphoreTest();
            //		SpinLockTest test = new SpinLockTest();
            //		ConcurrentStackTest test = new ConcurrentStackTest();
            //		ConcurrentQueueTest test = new ConcurrentQueueTest();
            test.Run();
            Console.ReadLine();
            Console.WriteLine("before Stop()");
            test.Stop();
        }
    }
}
