
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        // while (true)
        {

            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // connect
                socket.Connect(endPoint);
                Console.WriteLine($"connected to {socket.RemoteEndPoint?.ToString()}");

                for (int i = 0; i < 5; ++i)
                {
                    // send
                    byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World server:{i} ");
                    int sendBytes = socket.Send(sendBuff);
                }

                // recv
                byte[] recvBuff = new byte[1024];
                int recvByte = socket.Receive(recvBuff);
                string recvString = Encoding.UTF8.GetString(recvBuff, 0, recvByte);
                Console.WriteLine($"[From Server] {recvString}");

                Thread.Sleep(1000);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}