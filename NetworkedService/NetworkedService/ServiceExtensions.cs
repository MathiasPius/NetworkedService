using Microsoft.Extensions.DependencyInjection;
using NetworkedService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkedService
{
    public static class ServiceExtensions
    {
        public static void AddNetworkedScoping(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<INetworkedScope, NetworkedScope>();
        }

        public static void AddCommandSerializer<TSerializer>(this IServiceCollection serviceCollection)
            where TSerializer: class, ICommandSerializer
        {
            serviceCollection.AddScoped<ICommandSerializer, TSerializer>();
        }

        public static void AddCommandSerializer(this IServiceCollection serviceCollection, Type serializerType)
        {
            serviceCollection.AddScoped(typeof(ICommandSerializer), serializerType);
        }

        public static void AddCommandDeserializer<TDeserializer>(this IServiceCollection serviceCollection)
            where TDeserializer : class, ICommandDeserializer
        {
            serviceCollection.AddScoped<ICommandDeserializer, TDeserializer>();
        }

        public static void AddCommandDeserializer(this IServiceCollection serviceCollection, Type serializerType)
        {
            serviceCollection.AddScoped(typeof(ICommandDeserializer), serializerType);
        }

        public static void AddRemoteProcedureCaller<TCaller>(this IServiceCollection serviceCollection)
            where TCaller: class, IRemoteProcedureCaller
        {
            serviceCollection.AddScoped<IRemoteProcedureCaller, TCaller>();
        }

        public static void AddRemoteProcedureCaller(this IServiceCollection serviceCollection, Type remoteProcedureCaller)
        {
            serviceCollection.AddScoped(typeof(IRemoteProcedureCaller), remoteProcedureCaller);
        }
    }
}
