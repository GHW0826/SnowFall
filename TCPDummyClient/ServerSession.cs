using System.Net;
using System.Text;
using TCPServerCore;
using TCPServerExample.Packet;

namespace TCPDummyClient;

public class ServerSession : Session
{

    static unsafe void ToBytes(byte[] array, int offset, ulong value)
    {
        fixed (byte* ptr = &array[offset])
        {
            *(ulong*)ptr = value;
        }
    }


    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");

        // Packet packet = new Packet() { size = 4, packetId = 7 };
        PlayerInfoReq packet = new PlayerInfoReq()
        {
            playerId = 1001,
            name = "test_plater",
            skills = new() { 
                new() { id = 1, level = 5, duration = 0.1f, 
                    attributes = new() { new() { attr = 1 } } 
                },
                new() { id = 2, level = 10, duration = 20.1f,
                    attributes = new() { new() { attr = 2 } }
                },
                new() { id = 3, level = 15, duration = 15.1f,
                    attributes = new() { new() { attr = 3 } }
                }

            }
        };

        try
        {
            // for (int i = 0; i < 5; i++)
            {
                var sendBuff = packet.Write();
                if (sendBuff != null)
                    Send(sendBuff);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public override void OnDisConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected: {endPoint}");
    }

    // ex) 이동 패킷 (3, 2) 좌표로 이동 (15번 패킷)
    // 15 3 2
    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvString = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[from server]: {recvString}");
        return buffer.Count;
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"BytesTransferred: {numOfBytes}");
    }
}
