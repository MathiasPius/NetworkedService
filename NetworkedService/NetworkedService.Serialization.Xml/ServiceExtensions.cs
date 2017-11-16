using Microsoft.Extensions.DependencyInjection;

namespace NetworkedService.Serialization.Xml
{
    public static class ServiceExtensions
    {
        public static void AddXmlCommandSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCommandSerializer<XmlTranslator>();
        }
    }
}
