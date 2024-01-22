using DummyClient.Session;
using System.Net;
using TCPServerCore;

int DummyClientCount = 500;

Thread.Sleep(3000);
string host = Dns.GetHostName();
IPHostEntry ipHost = Dns.GetHostEntry(host);
IPAddress ipAddr = ipHost.AddressList[0];
IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

Connector connector = new Connector();

connector.Connect(endPoint, () => { return SessionManager.Instance.Generate(); }, DummyClientCount);

while (true)
{
    Thread.Sleep(10000);
}