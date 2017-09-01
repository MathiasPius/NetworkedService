using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Transport.Tcp
{
    public static class ServiceExtensions
    {
        public static void AddTcpClient<TInterface>(this IServiceCollection serviceCollection, string hostname, int port, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TInterface: class
        {
            var descriptor = new ServiceDescriptor(
                typeof(TInterface), 
                RemoteServiceFactory<TInterface>.Factory(Client.Factory(hostname, port)),
                serviceLifetime
            );

            serviceCollection.Add(descriptor);
        }
    }
}
