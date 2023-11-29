using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExample.ThreadTest;

[TestClass]
public class ThreadTest
{
    static void MainThread(object? state)
    {
    }

    [TestMethod]
    public void ThreadPoolTest()
    {
        Stopwatch t = new();
        
        t.Start();

        for (int i = 0; i < 10000; i++)
        {
            // 소규모 작업에 쓰자.
            ThreadPool.QueueUserWorkItem(MainThread);
        }

        t.Stop();

        Console.WriteLine(t.ElapsedMilliseconds);
    }

    static void SubMethod(object? state)
    {
        for (int i = 0; i < 5; ++i)
            Console.Write("Sub Method");
    }

    [TestMethod]
    public void ThreadPoolSetThreadTest()
    {
        Stopwatch t = new();

        t.Start();

        ThreadPool.SetMinThreads(1, 1);
        ThreadPool.SetMaxThreads(5, 5);

        // thread pool work number setting
        for (int i = 0; i < 4; ++i)
            ThreadPool.QueueUserWorkItem((obj) => { while (true) { } });

        ThreadPool.QueueUserWorkItem(SubMethod);


        t.Stop();

        Console.WriteLine(t.ElapsedMilliseconds);
    }

    [TestMethod]
    public void TaskTest()
    {
        Stopwatch t = new();

        t.Start();
        
        for (int i = 0; i < 5; ++i)
        {
            Task task = new Task(() => { while (true) { } }, TaskCreationOptions.LongRunning);
            // Task task = new Task(() => { while (true) { } });

            task.Start();
        }
        ThreadPool.QueueUserWorkItem(SubMethod);

        t.Stop();

        Console.WriteLine(t.ElapsedMilliseconds);
    }

    [TestMethod]
    public void UnitThread()
    {
        Stopwatch time = new();

        time.Start();

        for (int i = 0; i < 10000; i++)
        {
            Thread t = new Thread(MainThread);
            t.Start();
        }

        time.Stop();

        Console.WriteLine(time.ElapsedMilliseconds);
    }
}
