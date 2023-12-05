using System;
using System.Collections.Generic;
using TCPServerCore;
using TCPServerExample.Packet;

public class PacketManager
{

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {

        _onRecv.Add((ushort)PacketID.C_PlayerInfoReq, MakePacket<C_PlayerInfoReq>);
        _handler.Add((ushort)PacketID.C_PlayerInfoReq, PacketHandler.C_PlayerInfoReqHandler);
 

        _onRecv.Add((ushort)PacketID.C_Chat, MakePacket<C_Chat>);
        _handler.Add((ushort)PacketID.C_Chat, PacketHandler.C_ChatHandler);
 

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
