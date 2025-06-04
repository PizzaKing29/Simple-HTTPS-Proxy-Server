using System.Net;
using System.Net.Sockets;
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

            Socket Client = Listener.AcceptSocket();

            // await MakeHTTPSRequest();

            byte[] Data = new byte[200];
            int Size = Client.Receive(Data);
            Console.WriteLine("Recieved data: ");
            for (int i = 0; i < Size; i++)
            {
                Console.Write(Convert.ToChar(Data[i]));
            }
        }
        catch (Exception)
        {
            Console.WriteLine("There was an error trying to fetch a request");
            throw;
        }
    }

    /*async static Task MakeHTTPSRequest()
    {
        HttpClient HttpClient = new HttpClient();
        HttpResponseMessage Response = await HttpClient.GetAsync("https://www.google.com");
        string Content = await Response.Content.ReadAsStringAsync();
        //Console.WriteLine(Content);
        //Console.WriteLine(Response);
    }
    */
}