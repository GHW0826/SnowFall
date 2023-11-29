using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExample.ThreadTest;

[TestClass]
public class CacheTest
{
    [TestMethod]
    public void CacheTest1()
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        int[,] arr = new int[10000, 10000];
        for (int i = 0; i < 10000; ++i)
        {
            for (int j = 0; j < 10000; ++j)
            {
                arr[i, j] = 1;
            }
        }

        time.Stop();

        Console.WriteLine(time.ElapsedTicks);
    }

    [TestMethod]
    public void CacheTest2()
    {
        Stopwatch time = new Stopwatch();
        time.Start();

        int[,] arr = new int[10000, 10000];
        for (int i = 0; i < 10000; ++i)
        {
            for (int j = 0; j < 10000; ++j)
            {
                arr[j, i] = 1;
            }
        }

        time.Stop();

        Console.WriteLine(time.ElapsedTicks);
    }

    public void MemoryBarrierTest()
    {

        // 1) Full memory Barrier (ASM MFENCE, C# Thread.MemoryBarrier) : Store/Load 둘 다 막음
        // 2) Store Memory Barrier (ASM SFENCE) : Store만 막음
        // 3) Load Memory Barrier (ASM LFENCE) : Load만 막음
    }
}
