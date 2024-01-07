using System;
using Google.Protobuf.Protocol;
using TCPServerExample.Game.Room;
using TCPServerExample.Session;
using static System.Net.Mime.MediaTypeNames;

namespace TCPServerExample.Game
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Speed = 20;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
            Console.WriteLine($"DOTO OnDamaged : {damage}");
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
            Console.WriteLine($"DOTO OnDead");
        }
    }
}
