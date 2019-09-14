using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using F1Telemetry.Core.Packets;
using F1Telemetry.Core.SessionManagement.Events;
using F1TelemetryNetCore.Packets;

namespace F1Telemetry.Core.SessionManagement
{
    public interface IEventPublisher
    {
        IObservable<Event> Events { get; }
    }

    public sealed class SessionEventsPacketHandler: IPacketHandler, IEventPublisher
    {
        private readonly EventLoopScheduler _scheduler;
        private EventHandler<Event> _newPacketRead;

        public SessionEventsPacketHandler()
        {
            _scheduler = new EventLoopScheduler(ts => new Thread(ts) { Name = "SessionEventsPacketHandler event loop scheduler", IsBackground = true});
            Events = Observable
                .FromEventPattern<Event>(eh => _newPacketRead += eh, eh => _newPacketRead -= eh)
                .ObserveOn(_scheduler)
                .Select(ep => ep.EventArgs);
        }

        public IObservable<Event> Events { get; }

        public void OnPacketEventData(ref PacketEventData pEventData, PacketSource source)
        {
            var sc = pEventData.EventStringCode;
            Event sessionEvent = sc switch
            {
                "SSTA" => (Event)new SessionStart(pEventData.Header.SessionUID, DateTime.Now, pEventData.Header.SessionTime),
                "SEND" => (Event)new SessionEnd(pEventData.Header.SessionUID, DateTime.Now, pEventData.Header.SessionTime),
                _ => throw new InvalidOperationException($"Invalid session type {sc}")
            };

            _newPacketRead?.Invoke(this, sessionEvent);

        }

        public void OnPacketCarSetupData(ref PacketCarSetupData pCarSetup, PacketSource packetSource)
        {
        }

        public void OnPacketCarStatusData(ref PacketCarStatusData pCarStatus, PacketSource packetSource)
        {
        }

        public void OnPacketCarTelemetryData(ref PacketCarTelemetryData pTelemetry, PacketSource packetSource)
        {
        }

        public void OnPacketLapData(ref PacketLapData pLap, PacketSource packetSource)
        {
        }

        public void OnPacketMotionData(ref PacketMotionData pMotion, PacketSource packetSource)
        {
        }

        public void OnPacketParticipantsData(ref PacketParticipantsData pParticipants, PacketSource packetSource)
        {
        }

        public void OnPacketSessionData(ref PacketSessionData pSessionData, PacketSource _)
        {
            var timestamp = DateTime.Now;
            var sessionId = pSessionData.Header.SessionUID;
            var sessionTime = pSessionData.Header.SessionTime;

            var weather = new WeatherInfo(
                (Weather) pSessionData.Weather,
                pSessionData.TrackTemperature,
                pSessionData.AirTemperature,
                sessionId,
                timestamp,
                sessionTime);
            _newPacketRead?.Invoke(this, weather);

            var trackInfo = new TrackInfo(
                (Track) pSessionData.TrackId,
                pSessionData.TrackLength, 
                sessionId,
                timestamp,
                sessionTime);
            _newPacketRead?.Invoke(this, trackInfo);

            var sessionPause = pSessionData.GamePaused != 0
                ? (Event)new SessionPause(sessionId, timestamp, sessionTime)
                : (Event)new SessionResume(sessionId, timestamp, sessionTime);
            _newPacketRead?.Invoke(this, sessionPause);

            var sessionDuration = new SessionDuration(TimeSpan.FromSeconds(pSessionData.SessionDuration), sessionId, timestamp, sessionTime);
            _newPacketRead?.Invoke(this, sessionDuration);

            var sessionTimeLeft = new SessionTimeLeft(TimeSpan.FromSeconds(pSessionData.SessionTimeLeft), sessionId, timestamp, sessionTime);
            _newPacketRead?.Invoke(this, sessionTimeLeft);
        }
    }
}
