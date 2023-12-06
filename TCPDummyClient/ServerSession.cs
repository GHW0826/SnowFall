using System.Net;
using System.Text;
using TCPDummyClient.Packet;
using TCPServerCore;

namespace TCPDummyClient;

public class ServerSession : PacketSession
{
    static unsafe void ToBytes(byte[] array, int offset, ulong value)
    {
        fixed (byte* ptr = &array[offset])
        {
            *(ulong*)ptr = value;
        }
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");
    }

    public override void OnDisConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected: {endPoint}");
    }


    public override void OnSend(int numOfBytes)
    {
        // Console.WriteLine($"BytesTransferred: {numOfBytes}");
    }
}
