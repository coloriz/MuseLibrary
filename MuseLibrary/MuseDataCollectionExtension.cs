using System;
using System.Linq;
using System.Collections.Generic;

namespace InsLab.Muse
{
    using MuseDataCollection = Dictionary<MuseData, List<IMusePacket>>;

    public static class MuseDataCollectionExtension
    {
        public static MuseDataCollection SelectByTime(this MuseDataCollection data, TimeSpan start, TimeSpan span)
        {
            var end = start + span;
            var result = new MuseDataCollection();

            foreach (var d in data)
            {
                result[d.Key] = d.Value.Where(e => start.Ticks <= e.Timestamp && e.Timestamp <= end.Ticks).ToList();
            }

            return result;
        }

        public static List<T> ChangeType<T>(this List<IMusePacket> packets) where T : IMusePacket
        {
            var casted = new List<T>(packets.Capacity);
            packets.ForEach(e => casted.Add((T)e));

            return casted;
        }
    }
}
