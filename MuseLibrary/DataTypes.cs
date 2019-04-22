using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace InsLab.Muse
{
    [Flags]
    public enum MuseData
    {
        EEG = 0x1, NotchFilteredEEG = 0x2, Accelerometer = 0x4, Gyro = 0x8,
        // Absolute Band Powers
        DeltaAbsolute = 0x10, ThetaAbsolute = 0x20, AlphaAbsolute = 0x40, BetaAbsolute = 0x80, GammaAbsolute = 0x100,
        // Relative Band Powers
        DeltaRelative = 0x200, ThetaRelative = 0x400, AlphaRelative = 0x800, BetaRelative = 0x1000, GammaRelative = 0x2000,
        // Band Power Session Scores
        DeltaSessionScore = 0x4000, ThetaSessionScore = 0x8000, AlphaSessionScore = 0x10000, BetaSessionScore = 0x20000, GammaSessionScore = 0x40000,
        TouchingForehead = 0x80000, Horseshoe = 0x100000, IsGood = 0x200000, Blink = 0x400000, JawClench = 0x800000, Battery = 0x1000000, DrlRef = 0x2000000,
        // All Absolutes
        Absolutes = 0x1F0,
        // All Relatives
        Relatives = 0x3E00,
        // All Sessions
        Sessions = 0x7C000,
        All = 0x02FFFFFF
    }

    public static class MuseDataHelper
    {
        public static IEnumerable<MuseData> GetValues()
        {
            var museDataValues = (MuseData[])Enum.GetValues(typeof(MuseData));
            return museDataValues.Where((data) => !data.IsCombination());
        }
    }

    public static class MuseDataExtension
    {
        internal static bool IsCombination(this MuseData data)
        {
            switch (data)
            {
                case MuseData.Absolutes:
                case MuseData.Relatives:
                case MuseData.Sessions:
                case MuseData.All:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanConvertTo(this MuseData data, Type type)
        {
            switch (data)
            {
                case MuseData.EEG:
                case MuseData.NotchFilteredEEG:
                case MuseData.DeltaAbsolute:
                case MuseData.ThetaAbsolute:
                case MuseData.AlphaAbsolute:
                case MuseData.BetaAbsolute:
                case MuseData.GammaAbsolute:
                case MuseData.DeltaRelative:
                case MuseData.ThetaRelative:
                case MuseData.AlphaRelative:
                case MuseData.BetaRelative:
                case MuseData.GammaRelative:
                case MuseData.DeltaSessionScore:
                case MuseData.ThetaSessionScore:
                case MuseData.AlphaSessionScore:
                case MuseData.BetaSessionScore:
                case MuseData.GammaSessionScore:
                    return type == typeof(Channel);
                case MuseData.Accelerometer:
                case MuseData.Gyro:
                    return type == typeof(Direction);
                case MuseData.TouchingForehead:
                case MuseData.Horseshoe:
                case MuseData.IsGood:
                case MuseData.Blink:
                case MuseData.JawClench:
                case MuseData.Battery:
                case MuseData.DrlRef:
                case MuseData.All:
                default:
                    break;
            }

            return false;
        }
    }

    public interface IMusePacket
    {
        string TypeName { get; }
        long Timestamp { get; set; }
    }

    public class Channel : IMusePacket
    {
        public string TypeName { get; } = nameof(Channel);
        public long Timestamp { get; set; }
        public float TP9 { get; set; }
        public float AF7 { get; set; }
        public float AF8 { get; set; }
        public float TP10 { get; set; }

        public Channel(float tp9, float af7, float af8, float tp10, long timestamp)
        {
            TP9 = tp9;
            AF7 = af7;
            AF8 = af8;
            TP10 = tp10;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"TP9 = {TP9}, AF7 = {AF7}, AF8 = {AF8}, TP10 = {TP10}, timestamp = {Timestamp}";
        }
    }

    public class Direction : IMusePacket
    {
        public string TypeName { get; } = nameof(Direction);
        public long Timestamp { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Direction(float x, float y, float z, long timestamp)
        {
            X = x;
            Y = y;
            Z = z;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"X = {X}, Y = {Y}, Z = {Z}, timestamp = {Timestamp}";
        }
    }

    public enum SignalStatus
    {
        Good = 1, Mediocre = 2, Bad = 4
    }

    public class HeadbandStatus : IMusePacket
    {
        public string TypeName { get; } = nameof(HeadbandStatus);
        public long Timestamp { get; set; }
        public SignalStatus TP9 { get; set; }
        public SignalStatus AF7 { get; set; }
        public SignalStatus AF8 { get; set; }
        public SignalStatus TP10 { get; set; }
    }

    public class EEGQuality : IMusePacket
    {
        public string TypeName { get; } = nameof(EEGQuality);
        public long Timestamp { get; set; }
        public int TP9 { get; set; }
        public int AF7 { get; set; }
        public int AF8 { get; set; }
        public int TP10 { get; set; }
    }

    public class BatteryStatus : IMusePacket
    {
        public string TypeName { get; } = nameof(BatteryStatus);
        public long Timestamp { get; set; }
        public float StateOfCharge { get; set; }
        public float FuelGaugeBatteryVoltage { get; set; }
        public float Temperature { get; set; }
    }

    internal static class Map
    {
        private static Dictionary<string, MuseData> _addressToData = new Dictionary<string, MuseData>()
        {
            ["/eeg"] = MuseData.EEG,
            ["/notch_filtered_eeg"] = MuseData.NotchFilteredEEG,
            ["/acc"] = MuseData.Accelerometer,
            ["/gyro"] = MuseData.Gyro,
            ["/elements/delta_absolute"] = MuseData.DeltaAbsolute,
            ["/elements/theta_absolute"] = MuseData.ThetaAbsolute,
            ["/elements/alpha_absolute"] = MuseData.AlphaAbsolute,
            ["/elements/beta_absolute"] = MuseData.BetaAbsolute,
            ["/elements/gamma_absolute"] = MuseData.GammaAbsolute,
            ["/elements/delta_relative"] = MuseData.DeltaRelative,
            ["/elements/theta_relative"] = MuseData.ThetaRelative,
            ["/elements/alpha_relative"] = MuseData.AlphaRelative,
            ["/elements/beta_relative"] = MuseData.BetaRelative,
            ["/elements/gamma_relative"] = MuseData.GammaRelative,
            ["/elements/delta_session_score"] = MuseData.DeltaSessionScore,
            ["/elements/theta_session_score"] = MuseData.ThetaSessionScore,
            ["/elements/alpha_session_score"] = MuseData.AlphaSessionScore,
            ["/elements/beta_session_score"] = MuseData.BetaSessionScore,
            ["/elements/gamma_session_score"] = MuseData.GammaSessionScore,
            ["/elements/touching_forehead"] = MuseData.TouchingForehead,
            ["/elements/horseshoe"] = MuseData.Horseshoe,
            ["/elements/is_good"] = MuseData.IsGood,
            ["/elements/blink"] = MuseData.Blink,
            ["/elements/jaw_clench"] = MuseData.JawClench,
            ["/batt"] = MuseData.Battery,
            ["/drlref"] = MuseData.DrlRef
        };
        public static ImmutableDictionary<string, MuseData> AddressToData { get; } = _addressToData.ToImmutableDictionary();
    }
}
