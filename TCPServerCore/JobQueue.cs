using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerCore;

public interface IJobQueue
{
    public void Push(Action job);
}

public class JobQueue : IJobQueue
{
    Queue<Action> _jobQueue = new();
    object _lock = new();
    bool _flush = false;

    public void Push(Action job)
    {
        bool flush = false;
        lock (_lock)
        {
            _jobQueue.Enqueue(job);
            if (_flush == false)
                flush = _flush = true;

        }

        if (flush)
            Flush();
    }

    public void Flush()
    {
        while (true)
        {
            Action action = Pop();
            if (action == null)
                return;

            action.Invoke();
        }
    }

    Action Pop()
    {
        lock (_lock)
        {
            if (_jobQueue.Count == 0)
            {
                _flush = false;
                return null;
            }
            return _jobQueue.Dequeue();
        }
    }
}
