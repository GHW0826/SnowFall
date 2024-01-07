using System;
using System.Collections.Generic;
using TCPDummyClient.Packet;
using TCPServerCore;

public class PacketManager
{

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    PacketManager()
    {
        Register();
    }

    public void Register()
    {

        _onRecv.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
        _handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);
 

        _onRecv.Add((ushort)PacketID.S_BroadcaseEnterGame, MakePacket<S_BroadcaseEnterGame>);
        _handler.Add((ushort)PacketID.S_BroadcaseEnterGame, PacketHandler.S_BroadcastEnterGameHandler);
 

        _onRecv.Add((ushort)PacketID.S_BroadcastLeaveGame, MakePacket<S_BroadcastLeaveGame>);
        _handler.Add((ushort)PacketID.S_BroadcastLeaveGame, PacketHandler.S_BroadcastLeaveGameHandler);
 

        _onRecv.Add((ushort)PacketID.S_PlayerList, MakePacket<S_PlayerList>);
        _handler.Add((ushort)PacketID.S_PlayerList, PacketHandler.S_PlayerListHandler);
 

        _onRecv.Add((ushort)PacketID.S_BroadcastMove, MakePacket<S_BroadcastMove>);
        _handler.Add((ushort)PacketID.S_BroadcastMove, PacketHandler.S_BroadcastMoveHandler);
 

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
    public void HandlePacket(PacketSession session, IPacket packet)
    {
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}
