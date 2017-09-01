using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Serialization.Json;
using NetworkedService.Transport.Tcp;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NetworkedService.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Server side
            var serverServices = new ServiceCollection();
            serverServices.AddJsonCommandDeserializer();
            serverServices.AddSingleton<IMath, Math>();

            var serverSide = serverServices.BuildServiceProvider();

            var server = new RemoteServiceHost(new Server(IPAddress.Loopback, 5555, new JsonCommandDeserializer()));
            server.ExposeInterface<IMath>(serverSide);

            server.ListenAsync();

            // Client side
            var clientServices = new ServiceCollection();
            clientServices.AddNetworkedScoping();
            clientServices.AddJsonCommandSerializer();
            clientServices.AddTcpClient<IMath>("localhost", 5555);

            var clientSide = clientServices.BuildServiceProvider();

            while(true)
            {
                using (var scope = clientSide.CreateScope())
                {
                    var client = scope.ServiceProvider.GetService<IMath>();

                    // Read two numbers
                    Console.Write("> ");
                    var input = Console.ReadLine();
                
                    var data = input.Split(' ')
                        .Select(s => int.Parse(s))
                        .ToList();

                    // Use them
                    var val = client.Add(data[0], data[1]);
                    Console.WriteLine("Client: " + data[0] + " + " + data[1] + " = " + val);
                    Console.WriteLine("Client: " + data[0] + " - " + data[1] + " = " + client.Sub(data[0], data[1]));
                    Console.WriteLine("Client: Rand() = " + client.Rand());
                    client.Noop(val);
                }
            }
        }
    }
}
