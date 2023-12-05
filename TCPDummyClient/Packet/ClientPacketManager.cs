using System;
using System.Collections.Generic;
using TCPDummyClient.Packet;
using TCPServerCore;

public class PacketManager
{

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new();

    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new();

    public void Register()
    {

        _onRecv.Add((ushort)PacketID.S_Test, MakePacket<S_Test>);
        _handler.Add((ushort)PacketID.S_Test, PacketHandler.S_TestHandler);
 

        _onRecv.Add((ushort)PacketID.S_Chat, MakePacket<S_Chat>);
        _handler.Add((ushort)PacketID.S_Chat, PacketHandler.S_ChatHandler);
 

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
