using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using F1Telemetry.Core.Packets;
using F1TelemetryNetCore.Packets;

namespace F1Telemetry.Core.Observables
{
    public class PacketHandlerObservable: IPacketHandler
    {
        private EventHandler<Event> _newPacketRead;
        private readonly IObservable<Event> _events;
        private EventLoopScheduler _scheduler;

        public PacketHandlerObservable()
        {
            _scheduler = new EventLoopScheduler();
            _events = Observable
                .FromEventPattern<Event>(eh => _newPacketRead += eh, eh => _newPacketRead -= eh)
                .ObserveOn(_scheduler)
                .Select(ep => ep.EventArgs);
        }

        void IPacketHandler.OnPacketEventData(ref PacketEventData pEventData)
        {
            var sc = pEventData.EventStringCode;
            var sty = (sc switch
            {
                "SSTA" => SessionEvent.EventTypes.Start,
                "SEND" => SessionEvent.EventTypes.End,
                _ => throw new InvalidOperationException($"Invalid session type {sc}")
            });

            var sessionEvent = new SessionEvent(sty, pEventData.Header.SessionUID, DateTime.Now, pEventData.Header.SessionTime);
            _newPacketRead?.Invoke(
                this,
                sessionEvent);

        }

        void IPacketHandler.OnStop()
        {
        }

        void IPacketHandler.OnPacketSessionData(ref PacketSessionData pSessionData)
        {
            var timestamp = DateTime.Now;
            var sessionId = pSessionData.Header.SessionUID;
            var sessionTime = pSessionData.Header.SessionTime;
            var weather = new WeatherInfo(
                (Weather) pSessionData.Weather,
                pSessionData.TrackTemperature,
                pSessionData.AirTemperature);
            var trackInfo = new TrackInfo((Track) pSessionData.TrackId, pSessionData.TrackLength);
            var state = new SessionState(
                pSessionData.GamePaused != 0,
                pSessionData.NetworkGame != 0,
                pSessionData.IsSpectating != 0,
                TimeSpan.FromSeconds(pSessionData.SessionTimeLeft),
                TimeSpan.FromSeconds(pSessionData.SessionDuration));
            var sessionData = new Session(state, weather, trackInfo, sessionId, timestamp, sessionTime);
            _newPacketRead?.Invoke(this, sessionData);
        }

        public IObservable<Event> Events => _events;
    }
}
