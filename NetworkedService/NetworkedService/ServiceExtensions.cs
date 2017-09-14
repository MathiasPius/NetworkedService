using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService
{
    public static class ServiceExtensions
    {

        public static void AddCommandSerializer(this IServiceCollection serviceCollection, Type serializerType)
        {
            serviceCollection.AddSingleton(typeof(IRemoteProcedureSerializer), serializerType);
        }

        public static void AddCommandSerializer<TSerializer>(this IServiceCollection serviceCollection)
            where TSerializer : class, IRemoteProcedureSerializer
            => AddCommandSerializer(serviceCollection, typeof(TSerializer));


        public static void AddRemoteProcedureCaller<TCaller>(this IServiceCollection serviceCollection)
            where TCaller: class, IRemoteProcedureCaller
        {
            serviceCollection.AddSingleton<IRemoteProcedureCaller, TCaller>();
        }

        public static void AddRemoteProcedureCaller(this IServiceCollection serviceCollection, Type remoteProcedureCaller)
        {
            serviceCollection.AddScoped(typeof(IRemoteProcedureCaller), remoteProcedureCaller);
        }

        public static void AddRemoteServiceHost(this IServiceCollection serviceCollection, Action<RemoteServiceHostOptions> options)
        {
            serviceCollection.AddSingleton<RemoteServiceHost>();
            options(new RemoteServiceHostOptions(serviceCollection));
        }

        public static RemoteServiceHostOptions AddRemoteServiceHost(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<RemoteServiceHost>();
            return new RemoteServiceHostOptions(serviceCollection);
        }

        public static RemoteServiceHostConfiguration UseRemoteServiceHost(this IServiceProvider serviceProvider)
        {
            var server = serviceProvider.GetService<RemoteServiceHost>();
            return new RemoteServiceHostConfiguration(server);
        }
    }
}
