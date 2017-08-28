using System.Text;

using Newtonsoft.Json;

using NetworkedService.Interfaces;
using NetworkedService.Models;

namespace NetworkedService.Serialization.Json
{
    public class JsonCommandDeserializer : ICommandDeserializer
    {
        public RemoteCommand DeserializeCommand(byte[] rawData)
        {
            return JsonConvert.DeserializeObject<RemoteCommand>(Encoding.UTF8.GetString(rawData));
        }

        public byte[] SerializeResult(RemoteResult remoteResult)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(remoteResult));
        }
    }
}
