using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCPServerCore;

namespace TCPServerExample.Packet;

public class PacketManager
{

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {
        _onRecv.Add((ushort)PacketID.PlayerInfoReq, MakePacket<PlayerInfoReq>);
        _handler.Add((ushort)PacketID.PlayerInfoReq, PacketHandler.PlayerInfoReqHandler);
    }

    #region Singleton
    static PacketManager _instacne;
    public static PacketManager Instance
    {
        get
        {
            if (_instacne == null)
                _instacne = new();
            return _instacne;
        }
    }
    #endregion

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;
        // [size(2)][packetId(2)][ ... ]
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}
