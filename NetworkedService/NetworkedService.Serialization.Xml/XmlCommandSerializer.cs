using System.Xml.Serialization;
using System.IO;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;

namespace NetworkedService.Serialization.Xml
{
    public class XmlCommandSerializer : ICommandSerializer
    {
        public byte[] SerializeCommand(RemoteCommand remoteCommand)
        {
            var stream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(RemoteCommand));
            serializer.Serialize(stream, remoteCommand);

            stream.Flush();
            return stream.GetBuffer();
        }

        public RemoteResult DeserializeResult(byte[] remoteResult)
        {
            var stream = new MemoryStream(remoteResult);
            var serializer = new XmlSerializer(typeof(RemoteResult));

            return (RemoteResult)serializer.Deserialize(stream);
        }

        public object ConvertResult(object result, Type resultType)
        {
            throw new NotImplementedException();
        }
    }
}
