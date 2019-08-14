using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using F1TelemetryNetCore.Packets;

namespace F1Telemetry.Core.Observables
{
    public class Session: Event
    {
        public SessionState State { get; }
        public WeatherInfo Weather { get; }
        public TrackInfo Track { get; }

        public Session(
            SessionState state,
            WeatherInfo weather,
            TrackInfo track,
            ulong sessionId, DateTime timeStamp, float sessionTime)
            : base(sessionId, timeStamp, sessionTime)
        {
            State = state;
            Weather = weather;
            Track = track;
        }
    }

    public class TrackInfo
    {
        public Track Track { get; }
        public ushort Length { get; }

        public TrackInfo(Track track, ushort length)
        {
            Track = track;
            Length = length;    
        }
    }

    public enum Track: sbyte
    {
        Unknown = -1,
        Melbourne,
        PaulRicard,
        Shanghai,
        Sakhir,
        Catalunya,
        Monaco,
        Montreal,
        Silverstone,
        Hockenheim,
        Hungaroring,
        Spa,
        Monza,
        Singapore,
        Suzuka,
        AbuDhabi,
        Texas,
        Brazil,
        Austria,
        Sochi,
        Mexico,
        Baku,
        SakhirShort,
        SilverstoneShort,
        TexasShort,
        SuzukaShort
    }

    public class SessionState
    {
        public bool Paused { get; }
        public bool Online { get; }
        public bool Spectating { get; }
        public TimeSpan TimeLeft { get; }
        public TimeSpan Duration { get; }

        public SessionState(bool paused, bool online, bool spectating, TimeSpan timeLeft, TimeSpan duration)
        {
            Paused = paused;
            Online = online;
            Spectating = spectating;
            TimeLeft = timeLeft;
            Duration = duration;    
        }
    }

    public class WeatherInfo
    {
        public Weather Weather { get; }
        public sbyte TrackTemperature { get; }
        public sbyte AirTemperature { get; }

        public WeatherInfo(Weather weather, sbyte trackTemperature, sbyte airTemperature)
        {
            Weather = weather;
            TrackTemperature = trackTemperature;
            AirTemperature = airTemperature;    
        }
    }

    public enum Weather: byte
    {
        Clear,
        LightlyClouded,
        Overcast,
        LightRain,
        HeavyRain,
        Storm
    }
}
