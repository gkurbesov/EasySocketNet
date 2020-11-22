using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasySocketNet;
using EasySocketNet.Arguments;

namespace EasySocketNet.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //DefaultClient();
            await AsyncClient();
        }

        public static void DefaultClient()
        {
            var client = new TcpClient();
            client.OnChangeStatus += Client_OnChangeStatus;
            client.OnReceived += Client_OnReceived;

            client.Connect("localhost", 90);
            bool toExit = false;
            while (!toExit)
            {
                var msg = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    switch (msg)
                    {
                        case "connect":
                            client.Connect("localhost", 90);
                            break;
                        case "disconnect":
                            client.Disconnect();
                            break;
                        case "exit":
                            toExit = true;
                            break;
                        default:
                            var data = Encoding.UTF8.GetBytes(msg);
                            client.Send(data);
                            break;
                    }
                }
            }
        }

        public static async Task AsyncClient()
        {
            var client = new TcpClientAsync();
            client.OnChangeStatus += Client_OnChangeStatus;

            bool toExit = false;
            while (!toExit)
            {
                var msg = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    switch (msg)
                    {
                        case "connect":
                            var connectResult = await client.ConnectAsync("localhost", 90, CancellationToken.None);
                            Console.WriteLine($"Connect result = {connectResult}");
                            break;
                        case "disconnect":
                            await client.DisconnectAsync();
                            break;
                        case "receive":
                            var receiveResult = await client.ReceiveAsync(CancellationToken.None);
                            if(receiveResult != null)
                            {
                                var message = Encoding.UTF8.GetString(receiveResult);
                                Console.WriteLine($"Receive: {message}");
                            }
                            else
                            {
                                Console.WriteLine($"Receive error");
                            }
                            break;
                        case "exit":
                            await client.DisconnectAsync();
                            toExit = true;
                            break;
                        default:
                            var data = Encoding.UTF8.GetBytes(msg);
                            var sendResult = await client.SendAsync(data);
                            Console.WriteLine($"Send result = {sendResult}");
                            break;
                    }
                }
            }
        }

        private static void Client_OnReceived(object sender, ReceivedArgs e)
        {
            var message = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine($"Receive {e.RemoteEndPoint}: {message}");
        }

        private static void Client_OnChangeStatus(object sender, ClientStatusArgs e)
        {
            Console.WriteLine($"Change status = {e.Status}");
        }
    }
}
