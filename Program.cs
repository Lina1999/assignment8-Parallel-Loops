using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Parallel_Loops
{
    /// <summary>
    /// class Program.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ParallelLoopResult loopRes = new ParallelLoopResult();
            List<int> arr = new List<int>();
            for (int i = 0; i < 10; ++i)
                arr.Add(i);

            loopRes.ParallelFor(0, 10, i => Console.WriteLine(i));

            loopRes.ParallelForEach(arr, i => Console.WriteLine(i));

            loopRes.ParallelForEachWithOptions(arr, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
            {
                Console.WriteLine(i);
                //sleeping thread for making the result visible
                Thread.Sleep(1000);
            });

            loopRes.ParallelForEachWithOptions(arr, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            });

            loopRes.ParallelForEachWithOptions(arr, new ParallelOptions { MaxDegreeOfParallelism = 3 }, i =>
            {
                Console.WriteLine(i);
                Thread.Sleep(1000);
            });
        }
    }
}
