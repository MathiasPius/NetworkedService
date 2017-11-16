using NetworkedService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetworkedService.Models;
using System.IO;
using System.Xml.Serialization;

namespace NetworkedService.Serialization.Xml
{
    public class XmlTranslator : IRemoteProcedureSerializer
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

        public object ConvertObject(object result, Type resultType)
        {
            return result;
        }
    }
}
