using System.Text;

using Newtonsoft.Json;

using NetworkedService.Interfaces;
using NetworkedService.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using System;

namespace NetworkedService.Serialization.Json
{
    public class JsonCommandDeserializer : ICommandDeserializer
    {
        public object[] ConvertParameters(object[] parameters, Type[] parameterTypes)
        {
            return parameters
                .Zip(parameterTypes, (p, t) => {
                    if (p is JToken)
                    {
                        return ((JToken)p).ToObject(t);
                    }
                    
                    return p;
                })
                .ToArray();
        }

        public RemoteCommand DeserializeCommand(byte[] rawData)
        {
            var json = Encoding.UTF8.GetString(rawData);
            return JsonConvert.DeserializeObject<RemoteCommand>(json);
        }

        public byte[] SerializeResult(RemoteResult remoteResult)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(remoteResult));
        }
    }
}
