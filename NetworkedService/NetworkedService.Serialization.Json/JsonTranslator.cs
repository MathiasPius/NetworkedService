using NetworkedService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using NetworkedService.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace NetworkedService.Serialization.Json
{
    public class JsonTranslator : IRemoteProcedureSerializer
    {
        public object ConvertObject(object result, Type resultType)
        {
            if (result is JToken)
                return ((JToken)result).ToObject(resultType);

            return result;
        }

        public RemoteCommand DeserializeCommand(byte[] remoteCommand)
        {
            return JsonConvert.DeserializeObject<RemoteCommand>(Encoding.UTF8.GetString(remoteCommand));
        }

        public RemoteResult DeserializeResult(byte[] remoteResult)
        {
            return JsonConvert.DeserializeObject<RemoteResult>(Encoding.UTF8.GetString(remoteResult));
        }

        public byte[] SerializeCommand(RemoteCommand remoteCommand)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(remoteCommand));
        }

        public byte[] SerializeResult(RemoteResult remoteResult)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(remoteResult));
        }
    }
}
