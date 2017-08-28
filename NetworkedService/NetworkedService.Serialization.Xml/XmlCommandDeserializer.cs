﻿using System.Xml.Serialization;
using System.IO;

using NetworkedService.Interfaces;
using NetworkedService.Models;

namespace NetworkedService.Serialization.Xml
{
    public class XmlCommandDeserializer : ICommandDeserializer
    {
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