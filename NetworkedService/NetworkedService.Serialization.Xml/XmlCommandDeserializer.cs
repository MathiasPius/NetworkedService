using System.Xml.Serialization;
using System.IO;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;

namespace NetworkedService.Serialization.Xml
{
    public class XmlCommandDeserializer : ICommandDeserializer
    {
        public object[] ConvertParameters(object[] parameters, Type[] parameterTypes)
        {
            throw new NotImplementedException();
        }

        public RemoteCommand DeserializeCommand(byte[] rawData)
        {
            var stream = new MemoryStream(rawData);
            var serializer = new XmlSerializer(typeof(RemoteCommand));

            return (RemoteCommand)serializer.Deserialize(stream);
        }

        public byte[] SerializeResult(RemoteResult remoteResult)
        {
            var stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(RemoteResult));
            serializer.Serialize(stream, remoteResult);

            stream.Flush();
            return stream.GetBuffer();
        }
    }
}
