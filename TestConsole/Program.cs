using AndreyPro.Common;
using System;
using System.Threading;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TestList();
            Console.ReadKey();
        }

        static void TestList()
        {
            var list = new DictionaryWrapper<int, int>((x) => x);
            list.Modify += (t, x) =>
            {

            };
            list.AddOrUpdate(1);
            list.AddOrUpdate(2);
            list.AddOrUpdate(1);
            list.RemoveByKey(1);
            foreach (var item in list)
            {

            }
        }

        static void Run()
        {
            var key = "1";
            var t = MemoryCache2.Instance.GetOrAddConcurrent(key, () =>
            {
                Thread.Sleep(10000);
                return 1;
            });
            var t1 = MemoryCache2.Instance.Get(key);
            var t2 = MemoryCache2.Instance.Get(key, "1");
            var t3 = MemoryCache2.Instance.Get(key, "2");
        }
    }
}
