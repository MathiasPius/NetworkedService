using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService.Transport.NetMQ
{
    public static class ServiceExtensions
    {
        public static void AddNetMQClient<TInterface>(this IServiceCollection serviceCollection, string endpoint)
            where TInterface: class
        {
            serviceCollection.AddScoped<TInterface>(RemoteServiceFactory<TInterface>.Factory(
                Client.Factory(endpoint)
            ));
        }
    }
}
