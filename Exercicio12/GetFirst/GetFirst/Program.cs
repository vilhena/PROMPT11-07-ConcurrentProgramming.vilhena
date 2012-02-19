using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GetFirst
{
    static class TaskExtensions
    {
        public static Task<T> WithTimeout<T>(this Task<T> task, int timeout)
        {
            //if (task.Status == TaskStatus.Created)
            //    return new Task<T>(() =>
            //                           {
            //                               return task.WithTimeout(timeout);
            //                           });
            //else
            //{
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

            Timer t = new Timer((_) =>
                                    {
                                        tcs.TrySetException(new TimeoutException());
                                    }, null, timeout, Timeout.Infinite);
            
            task.ContinueWith((_) =>
                                  {
                                      t.Dispose();
                                      if (task.IsFaulted)
                                      {
                                          tcs.SetException(task.Exception);
                                      }
                                      if (task.IsCanceled)
                                      {
                                          tcs.SetCanceled();
                                      }
                                      else
                                          tcs.TrySetResult(task.Result);
                                  });

            return tcs.Task;
            //}
        }
    }
  

    class Program
    {
        public static Task<T> GetFirstResult<T>(params Task<T>[] tasks)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            int count = tasks.Length;


            tasks.AsParallel().ForAll((t) =>
                                          {
                                              t.ContinueWith((_) =>
                                                                 {

                                                                     if (t.IsFaulted)
                                                                     {
                                                                         if (Interlocked.Decrement(ref count) == 0)
                                                                         {
                                                                             tcs.SetException(t.Exception);
                                                                         }
                                                                     }
                                                                     else if (t.IsCompleted)
                                                                         tcs.TrySetResult(t.Result);

                                                                         
                                                                 });
                                          });

            return tcs.Task;
        }

        




        static void Main(string[] args)
        {
            var t1 = new Task<int>(() =>
                                       {
                                           Thread.Sleep(5000);
                                           throw new AbandonedMutexException();
                                           return 1;
                                       });
            var t2 = new Task<int>(() =>
                                       {
                                           
                                           Thread.Sleep(10000);
                                           throw new AccessViolationException();
                                           return 2;
                                       });
            var t3 = new Task<int>(() =>
            {
                while (true)
                {
                    
                    Thread.Sleep(1000);   
                    throw new AggregateException();
                }
                return 2;
            });

            //t1.Start();

            //t2.Start();

            //t3.Start();



            //var t = GetFirstResult(t1, t2, t3);

            //try
            //{

            //    int result = t.Result;
            //    Console.WriteLine(result);

            //}
            //catch (AggregateException e)
            //{
            //    Console.WriteLine(e.InnerExceptions.First().InnerException.GetType().ToString());
            //}
            //catch
            //{
            //    Console.WriteLine("General");
            //}
            
            //Console.ReadLine();

            var tt = Task.Factory.StartNew(() =>
                                      {
                                          Thread.Sleep(10000);
                                          return 1000;
                                      }).WithTimeout(3000);

            var res = tt.Result;
            
        }
    }
}
