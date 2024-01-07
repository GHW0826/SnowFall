using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerExample.Drecated;
using TCPServerExample.Session;

namespace TCPServerExample
{

    interface ITask
    {
        void Execute();
    }

    public class BroadcastTask : ITask
    {
        GameRoom _room;
        ClientSession _session;
        string _chat;
        public BroadcastTask(GameRoom room, ClientSession session, string chat)
        {
            _room = room;
            _session = session;
            _chat = chat;
        }

        public void Execute()
        {
            //_room.Broadcast(_session, _chat);
        }
    }


    public class TaskQueue
    {
        Queue<ITask> _queue = new();
    }



}
