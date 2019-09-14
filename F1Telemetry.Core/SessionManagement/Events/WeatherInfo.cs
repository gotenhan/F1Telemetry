using System;

namespace F1Telemetry.Core.SessionManagement.Events
{
    public sealed class WeatherInfo: Event
    {
        public Weather Weather { get; }
        public sbyte TrackTemperature { get; }
        public sbyte AirTemperature { get; }

        public WeatherInfo(Weather weather, sbyte trackTemperature, sbyte airTemperature, ulong sessionId, DateTime timeStamp, float sessionTime): base(sessionId, timeStamp, sessionTime)
        {
            Weather = weather;
            TrackTemperature = trackTemperature;
            AirTemperature = airTemperature;    
        }

        private bool Equals(WeatherInfo other)
        {
            return Weather == other.Weather && TrackTemperature == other.TrackTemperature && AirTemperature == other.AirTemperature;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is WeatherInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Weather;
                hashCode = (hashCode * 397) ^ TrackTemperature.GetHashCode();
                hashCode = (hashCode * 397) ^ AirTemperature.GetHashCode();
                return hashCode;
            }
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