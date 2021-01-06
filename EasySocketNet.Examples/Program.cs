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
            DefaultClient();
        }

        public static void DefaultClient()
        {
            var client = new TcpClient();
            client.OnChangeStatus += Client_OnChangeStatus;
            client.OnReceive += Client_OnReceived;

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
