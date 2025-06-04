using System.Net;
using System.Net.Sockets;

class Proxy
{
    public static void Main()
    {
        IPAddress IpAddress = IPAddress.Loopback; // The default loopback IP is 127.0.0.1
        TcpListener Listener = new TcpListener(IpAddress, 8888);

        Listener.Start();

        Console.WriteLine($"server is listening on IP/Port{Listener.LocalEndpoint}");
    }
}