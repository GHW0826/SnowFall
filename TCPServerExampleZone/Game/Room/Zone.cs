using Google.Protobuf.Protocol;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCPServerExampleZone.Game
{
    public class Zone
    {
        public int IndexY { get; private set; }
        public int IndexX { get; private set; }

        public HashSet<Player> Players { get; set; } = new();
        public HashSet<Monster> Monsters { get; set; } = new();
        public HashSet<Projectile> Projectiles { get; set; } = new();



        public Zone(int y, int x)
        {
            IndexY = y;
            IndexX = x;
        }

        public void Remove(GameObject gameObject)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            switch (type)
            {
                case GameObjectType.Player:
                    Players.Remove((Player)gameObject);
                    break;
                case GameObjectType.Monster:
                    Monsters.Remove((Monster)gameObject);
                    break;
                case GameObjectType.Projectile:
                    Projectiles.Remove((Projectile)gameObject);
                    break;
            }
        }

        public Player FindOnePlayer(Func<Player, bool> condition)
        {
            foreach (Player p in Players)
            {
                if (condition.Invoke(p))
                {
                    return p;
                }
            }
            return null;
        }

        public List<Player> FindAllPlayer(Func<Player, bool> condition)
        {
            List<Player> findList = new();
            foreach (Player p in Players)
            {
                if (condition.Invoke(p))
                    findList.Add(p);
            }

            return findList;
        }
    }
}
