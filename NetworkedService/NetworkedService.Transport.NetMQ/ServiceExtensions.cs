using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Transport.NetMQ
{
    public static class ServiceExtensions
    {

        public static void AddNetMQClient<TInterface>(this IServiceCollection serviceCollection, string endpoint, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
            where TInterface: class
        {
            var descriptor = new ServiceDescriptor(
                typeof(TInterface), 
                RemoteServiceFactory<TInterface>.Factory(Client.Factory(endpoint)),
                serviceLifetime
            );

            serviceCollection.Add(descriptor);
        }
    }
}
