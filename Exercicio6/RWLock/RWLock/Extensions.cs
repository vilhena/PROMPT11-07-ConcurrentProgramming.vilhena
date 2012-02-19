using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RWLock
{
    public static class Extensions
    {
        public static void Wait(this object o)
        {
            Monitor.Wait(o);
        }

        public static void LockEnter(this object o)
        {
            Monitor.Enter(o);
        }

        public static void LockExit(this object o)
        {
            Monitor.Exit(o);
        }

        public static void Pulse(this object o)
        {
            Monitor.Pulse(o);
        }

        public static void PulseAll(this object o)
        {
            Monitor.PulseAll(o);
        }




        public static void SafeLock(this object o, Action action)
        {
            bool exception = false;
            while (true)
            {
                try
                {
                    o.LockEnter();
                    break;
                }
                catch (ThreadInterruptedException)
                {
                    exception = true;
                }
            }

            try
            {
                action();
            }
            finally
            {
                if (exception)
                    Thread.CurrentThread.Interrupt();
                o.LockExit();
            }
        }


        public class MyAsyncResult<TIn, TOut> : IAsyncResult
        {
            public TOut Out { get; set; }
            private readonly WaitHandle _handle;

            public MyAsyncResult(object state)
            {
                AsyncState = state;
                _handle = new ManualResetEvent(false);

            }

            public bool IsCompleted { get; set;}

            public WaitHandle AsyncWaitHandle
            {
                get { return _handle; }
            }

            public object AsyncState{ get; set;}
            

            public bool CompletedSynchronously
            {
                get { return false; }
            }
        }


        public static IAsyncResult BeginCall<TIn, TOut>(this Func<TIn, TOut> func, TIn arg, AsyncCallback callback, Object state)
        {
            var ar = new MyAsyncResult<TIn, TOut>(state);

            ThreadPool.QueueUserWorkItem((_) =>
                                             {  
                                                 ar.Out = func(arg);
                                                 ar.IsCompleted = true;
                                                 if (callback != null)
                                                     callback(ar);
                                                 ((ManualResetEvent) ar.AsyncWaitHandle).Set();
                                             });

            return ar;
        }

        public static TOut EndCall<TIn, TOut>(this Func<TIn, TOut> func, IAsyncResult iar)
        {
            var myIar = iar as MyAsyncResult<TIn, TOut>;
            if(myIar != null)
            {
                if (!myIar.IsCompleted)
                {
                    myIar.AsyncWaitHandle.WaitOne();
                }
                return myIar.Out;
            }

            throw new InvalidCastException();
        }

    }
}
