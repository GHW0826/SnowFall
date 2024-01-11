using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using TCPServerExample.Data;
using TCPServerExample.DB;
using TCPServerExample.Game.Object;
using TCPServerExample.Game.Room;

namespace TCPServerExample.Game
{
    public partial class GameRoom : JobSerializer
    {
        public void HandleEquipItem(Player player, C_EquipItem equipPacket)
        {
            if (player == null)
                return;
            
            player.HandleEquipItem(equipPacket);
        }
    }
}
