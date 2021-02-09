using AndreyPro.Common;
using System;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
            Run();
            
            Task.Run(Run);
            Task.Run(Run);
            Console.ReadKey();
        }

        static void Run()
        {
            var key = "1";
            var t = MemoryCache2.Instance.GetOrAddConcurrent(key, () =>
            {
                //Thread.Sleep(10000);
                return new object();
            });
            var t1 = MemoryCache2.Instance.Get(key);
            var t2 = MemoryCache2.Instance.Get(key, "1");
            var t3 = MemoryCache2.Instance.Get(key, "2");
        }
    }
}
