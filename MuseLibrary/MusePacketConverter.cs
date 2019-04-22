using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsLab.Muse
{
    internal class MusePacketConverter : JsonConverter<IMusePacket>
    {
        public override IMusePacket ReadJson(JsonReader reader, Type objectType, IMusePacket existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = serializer.Deserialize<JToken>(reader) as JObject;

            string typeName = obj["TypeName"]?.ToString();

            switch (typeName)
            {
                case nameof(Channel):
                    return obj.ToObject<Channel>();
                case nameof(Direction):
                    return obj.ToObject<Direction>();
                default:
                    break;
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, IMusePacket value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
