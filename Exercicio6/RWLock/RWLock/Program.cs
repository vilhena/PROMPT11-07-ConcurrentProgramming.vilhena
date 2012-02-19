using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RWLock
{
    class Program
    {
        static void Main(string[] args)
        {
            bool ok = false;

            Func<int, bool> test = (i) =>
                                       {
                                           Thread.Sleep(5000);
                                           return i < 0;
                                       };

            var ar = test.BeginCall(10, callback, new object());

            var result = test.EndCall(ar);
        }


        static public void callback(object o)
        {
            Console.WriteLine("callback");
        }
    }
}
