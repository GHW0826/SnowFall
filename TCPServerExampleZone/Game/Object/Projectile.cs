using Google.Protobuf.Protocol;

namespace TCPServerExampleZone.Game
{
    public class Projectile : GameObject
    {
        public Data.Skill Data { get; set; }

        public Projectile()
        {
            ObjectType = GameObjectType.Projectile;
        }

        public override void Update()
        {
        }
    }
}
