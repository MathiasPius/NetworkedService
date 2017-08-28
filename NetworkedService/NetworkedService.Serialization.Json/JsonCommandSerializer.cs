using System.Text;

using Newtonsoft.Json;

using NetworkedService.Interfaces;
using NetworkedService.Models;

namespace NetworkedService.Serialization.Json
{
    public class JsonCommandSerializer : ICommandSerializer
    {
        public byte[] SerializeCommand(RemoteCommand remoteCommand)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(remoteCommand));
        }

        public RemoteResult DeserializeResult(byte[] remoteResult)
        {
            return JsonConvert.DeserializeObject<RemoteResult>(Encoding.UTF8.GetString(remoteResult));
        }
    }
}
