using System.Text;

using Newtonsoft.Json;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using System;
using Newtonsoft.Json.Linq;

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

        public object ConvertResult(object result, Type resultType)
        {
            return ((JToken)result).ToObject(resultType);
        }
    }
}
