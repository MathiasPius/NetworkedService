using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Serialization.Xml;
using NetworkedService.Transport.NetMQ;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NetworkedService.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            // Server side
            var serverServices = new ServiceCollection();
            serverServices.AddXmlCommandDeserializer();
            serverServices.AddSingleton<IMath, Math>();

            var serverSide = serverServices.BuildServiceProvider();

            var server = new RemoteServiceHost(new Server("tcp://localhost:5555", new XmlCommandDeserializer()));
            server.ExposeInterface<IMath>(serverSide);

            server.ListenAsync();

            // Client side
            var clientServices = new ServiceCollection();
            clientServices.AddNetworkedScoping();
            clientServices.AddXmlCommandSerializer();
            clientServices.AddNetMQClient<IMath>("tcp://localhost:5555");

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
