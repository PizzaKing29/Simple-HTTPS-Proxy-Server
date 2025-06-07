using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Proxy
{
    public static async Task Main()
    {
        // Change IP Address and Port here
        string InternetProtocol = "127.0.0.1";
        int Port = 8888;

        try
        {
            IPAddress IpAddress = IPAddress.Parse(InternetProtocol);
            TcpListener Listener = new TcpListener(IpAddress, Port);
            Listener.Start();

            Console.WriteLine($"server is listening on IP/Port {Listener.LocalEndpoint}");

            while (true)
            {
                Socket Client = await Listener.AcceptSocketAsync();
                _ = HandleClient(Client);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine("There was an error trying to fetch a request");
            Console.WriteLine(e.Message);
        }
    }

    public static async Task HandleClient(Socket Client)
    {
        try
        {
            byte[] Data = new byte[1024];
            string Output = "";
            int Size = await Client.ReceiveAsync(Data);

            for (int i = 0; i < Size; i++)
            {
                Output += Convert.ToChar(Data[i]);
            }
            Console.WriteLine($"Recieved data: {Output} ");


            var Match = Regex.Match(Output, @"CONNECT\s+(?<address>[^\s]+)");
            if (Match.Success)
            {
                string TargetAddress = Match.Groups["address"].Value;
                string[] Parts = TargetAddress.Split(':');
                string Domain = Parts[0];
                int DNSPort = int.Parse(Parts[1]);


                IPAddress[] Addresses = Dns.GetHostAddresses(Domain);
                IPAddress IpToUse = Addresses.FirstOrDefault(Ip => Ip.AddressFamily == AddressFamily.InterNetwork)?? Addresses[0]; // fallback to any if no IPv4 found


                IPEndPoint EndPoint = new IPEndPoint(IpToUse, DNSPort);
                Socket Target = new Socket(IpToUse.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                await Target.ConnectAsync(EndPoint);
                Console.WriteLine($"Connected to {Domain} ({IpToUse}:{DNSPort})");

                // send HTTP 200 Connection
                byte[] ConnectResponse = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection established\r\n\r\n");
                await Client.SendAsync(ConnectResponse);

                NetworkStream TargetStream = new NetworkStream(Target);

                Task ClientToTarget = RelayAsync(Client, Target);
                Task TargetToClient = RelayAsync(Target, Client);

                await Task.WhenAny(ClientToTarget, TargetToClient);

                // When one side disconnects, close both sockets
                Client.Close();
                Target.Close();

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

    public static async Task RelayAsync(Socket From, Socket To)
    {
        try
        {
            while (true)
            {
                Byte[] Buffer = new byte[4096];
                int BytesRead = await From.ReceiveAsync(Buffer, SocketFlags.None);
                if (BytesRead == 0)
                    break;

                await To.SendAsync(new ArraySegment<byte>(Buffer, 0, BytesRead), SocketFlags.None);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}