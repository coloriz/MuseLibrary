using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InsLab.Muse
{
    using MuseDataCollection = Dictionary<MuseData, List<IMusePacket>>;

    public static class MuseUtil
    {
        public static MuseDataCollection ReadMuseData(string path)
        {
            var jsonString = File.ReadAllText(path);
            var jsonObj = JObject.Parse(jsonString);

            foreach (var property in jsonObj.Properties().ToList())
            {
                // it's not the property of MuseData if parsing fails
                if (!Enum.TryParse(property.Name, out MuseData result))
                {
                    property.Remove();
                }
            }

            var serializer = new JsonSerializer();
            serializer.Converters.Add(new MusePacketConverter());

            var museDataCollection = jsonObj.ToObject<MuseDataCollection>(serializer);

            return museDataCollection;
        }
    }
}
