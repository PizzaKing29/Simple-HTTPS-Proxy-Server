using Microsoft.Extensions.Hosting;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

public class ProxyService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string internetProtocol = "127.0.0.1";
        int port = 8888;

        IPAddress ipAddress = IPAddress.Parse(internetProtocol);
        TcpListener listener = new TcpListener(ipAddress, port);
        listener.Start();

        Console.WriteLine($"server is listening on IP/Port {listener.LocalEndpoint}");

        while (!stoppingToken.IsCancellationRequested)
        {
            Socket client = await listener.AcceptSocketAsync();
            _ = HandleClient(client);
        }
    }

    public async Task HandleClient(Socket client)
    {
        try
        {
            byte[] data = new byte[1024];
            string output = "";
            int size = await client.ReceiveAsync(data);

            for (int i = 0; i < size; i++)
            {
                output += Convert.ToChar(data[i]);
            }
            Console.WriteLine($"Recieved data: {output} ");

            var match = Regex.Match(output, @"CONNECT\s+(?<address>[^\s]+)");
            if (match.Success)
            {
                string targetAddress = match.Groups["address"].Value;
                string[] parts = targetAddress.Split(':');
                string domain = parts[0];
                int dnsPort = int.Parse(parts[1]);

                IPAddress[] addresses = Dns.GetHostAddresses(domain);
                IPAddress ipToUse = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork) ?? addresses[0];

                IPEndPoint endPoint = new IPEndPoint(ipToUse, dnsPort);
                Socket target = new Socket(ipToUse.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await target.ConnectAsync(endPoint);
                Console.WriteLine($"Connected to {domain} ({ipToUse}:{dnsPort})");

                byte[] connectResponse = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
                await client.SendAsync(connectResponse);

                Task clientToTarget = RelayAsync(client, target);
                Task targetToClient = RelayAsync(target, client);

                await Task.WhenAny(clientToTarget, targetToClient);

                try
                {
                    client.Shutdown(SocketShutdown.Both);
                    target.Shutdown(SocketShutdown.Both);
                    client.Close();
                    target.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("No CONNECTION target has been found");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public async Task RelayAsync(Socket from, Socket to)
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = await from.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRead == 0) break;
                await to.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}