using System;
using System.Text;

namespace EasySocketNet.Examples.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new EasySocketNet.TcpServer();

            server.OnChangeStatus += Server_OnChangeStatus;
            server.OnClientConnect += Server_OnClientConnect;
            server.OnClientDisconnect += Server_OnClientDisconnect;
            server.OnReceive += Server_OnReceive;

            bool job = true;
            while (job)
            {
                Console.Write("Enter command: ");
                var input = Console.ReadLine().Trim();
                if (!string.IsNullOrWhiteSpace(input))
                {
                    var arg = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    switch (arg[0])
                    {
                        case "start":
                            if (arg.Length == 2)
                                server.Start(int.TryParse(arg[1], out var port) ? port : 90);
                            else
                                server.Start(90);
                            break;
                        case "stop":
                            server.Stop();
                            break;
                        case "kick":
                            if (arg.Length == 2 && int.TryParse(arg[1], out var kickClient))
                                server.Kick(kickClient);
                            break;
                        case "send":
                            if (arg.Length == 3 && int.TryParse(arg[1], out var sendClient))
                            {
                                var msg = Encoding.UTF8.GetBytes(arg[2]);
                                server.Send(sendClient, msg);
                            }
                            break;
                        case "online":
                            Console.WriteLine($"Onine clients: {server.Online}");
                            break;
                        case "get":
                            if (arg.Length == 1)
                            {
                                Console.WriteLine("Clients on server:");
                                foreach (var client in server.GetClients())
                                    Console.WriteLine($"\t {client.ClientId} \t {client.RemoteEndPoint}");

                            }
                            else if (arg.Length == 2 && int.TryParse(arg[1], out var getClient))
                            {
                                var client = server.GetClient(getClient);
                                if (client != null)
                                    Console.WriteLine($"\t {client.ClientId} \t {client.RemoteEndPoint}");
                                else
                                    Console.WriteLine($"\t {getClient} - not found");
                            }
                            break;
                        case "exit":
                            job = false;
                            break;
                    }
                }
            }

            Console.WriteLine("press Enter to exit...");
            Console.ReadLine();
        }

        private static void Server_OnReceive(object sender, Arguments.ReceivedArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Data);
            Console.WriteLine($"Receive message from {e.RemoteEndPoint}({e.ClientId}):\r\n\t{msg}");
        }

        private static void Server_OnClientDisconnect(object sender, Arguments.ConnectionArgs e)
        {
            Console.WriteLine($"Disconnected client: {e.ClientId}");
        }

        private static void Server_OnClientConnect(object sender, Arguments.ConnectionArgs e)
        {
            Console.WriteLine($"Connected new client: {e.ClientId}");
        }

        private static void Server_OnChangeStatus(object sender, Arguments.ServerStatusArgs e)
        {
            Console.WriteLine($"Server status = {e.Status}");
        }
    }
}
