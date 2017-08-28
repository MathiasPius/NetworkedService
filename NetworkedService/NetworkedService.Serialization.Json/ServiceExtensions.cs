using Microsoft.Extensions.DependencyInjection;

namespace NetworkedService.Serialization.Json
{
    public static class ServiceExtensions
    {
        public static void AddJsonCommandSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCommandSerializer<JsonCommandSerializer>();
        }

        public static void AddJsonCommandDeserializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCommandDeserializer<JsonCommandDeserializer>();
        }
    }
}
