using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Proxy
{
    public static async Task Main()
    {
        // Change IP Address and Port here
        string internetProtocol = "127.0.0.1";
        int port = 8888;

        try
        {
            IPAddress ipAddress = IPAddress.Parse(internetProtocol);
            TcpListener listener = new TcpListener(ipAddress, port);
            listener.Start();

            Console.WriteLine($"server is listening on IP/Port {listener.LocalEndpoint}");

            while (true)
            {
                Socket client = await listener.AcceptSocketAsync();
                _ = HandleClient(client);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("There was an error trying to fetch a request");
            Console.WriteLine(e.Message);
        }
    }

    public static async Task HandleClient(Socket client)
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
                IPAddress ipToUse = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork)?? addresses[0]; // fallback to any if no IPv4 found


                IPEndPoint endPoint = new IPEndPoint(ipToUse, dnsPort);
                Socket target = new Socket(ipToUse.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await target.ConnectAsync(endPoint);
                Console.WriteLine($"Connected to {domain} ({ipToUse}:{dnsPort})");

                // send HTTP 200 Connection
                byte[] connectResponse = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
                await client.SendAsync(connectResponse);

                NetworkStream targetStream = new NetworkStream(target);

                Task clientToTarget = RelayAsync(client, target);
                Task targetToClient = RelayAsync(target, client);

                await Task.WhenAny(clientToTarget, targetToClient);

                // When one side disconnects, close both sockets
                client.Close();
                target.Close();

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

    public static async Task RelayAsync(Socket from, Socket to)
    {
        try
        {
            while (true)
            {
                Byte[] buffer = new byte[4096];
                int bytesRead = await from.ReceiveAsync(buffer, SocketFlags.None);
                if (bytesRead == 0)
                    break;

                await to.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead), SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}