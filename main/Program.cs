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

            Socket Client = await Listener.AcceptSocketAsync();

            byte[] Data = new byte[200];
            string Output = "";
            int Size = await Client.ReceiveAsync(Data);
            Console.WriteLine("Recieved data: ");
            for (int i = 0; i < Size; i++)
            {
                // Console.Write(Convert.ToChar(Data[i]));
                Output += Convert.ToChar(Data[i]);
            }

            var Match = Regex.Match(Output, @"CONNECT\s+(?<address>[^\s]+)");

            if (Match.Success)
            {
                string TargetAddress = Match.Groups["address"].Value;
                Console.WriteLine(TargetAddress);
            }
            else
            {
                Console.WriteLine("No CONNECTION target has been found");
            }


        }
        catch (Exception)
        {
            Console.WriteLine("There was an error trying to fetch a request");
            throw;
        }
    }
}