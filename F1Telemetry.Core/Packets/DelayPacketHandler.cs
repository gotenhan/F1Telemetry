using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using F1TelemetryNetCore.Packets;
using Microsoft.VisualBasic.CompilerServices;

namespace F1Telemetry.Core.Packets
{
    public class DelayScale
    {
        public DelayScale(float scale)
        {
            Scale = scale;
        }

        public float Scale { get; set; }
        public bool Enabled { get; set; }

        public static implicit operator float(DelayScale scale)
        {
            return scale.Scale;
        }

        public static implicit operator DelayScale(float scale)
        {
            return new DelayScale(scale);
        }
    }

    public class DelayPacketHandler : IPacketHandler
    {
        private readonly DelayScale _delayScale;
        private Dictionary<ulong, float> _sessionLastTimes = new Dictionary<ulong, float>();

        public DelayPacketHandler(DelayScale delayScale)
        {
            _delayScale = delayScale;
        }

        public void OnPacketEventData(ref PacketEventData pEventData, PacketSource packetSource)
        {
            if (pEventData.EventStringCode == "SSTA")
            {
                _sessionLastTimes[pEventData.Header.SessionUID] = 0.0f;
            }
            else
            {
                _sessionLastTimes.Remove(pEventData.Header.SessionUID);
            }

        }

        public void OnPacketSessionData(ref PacketSessionData pSessionData, PacketSource packetSource)
        {
            Delay(pSessionData.Header.SessionUID, pSessionData.Header.SessionTime, packetSource);
        }

        public void OnPacketCarSetupData(ref PacketCarSetupData pCarSetup, PacketSource packetSource)
        {
            Delay(pCarSetup.Header.SessionUID, pCarSetup.Header.SessionTime, packetSource);
        }

        public void OnPacketCarStatusData(ref PacketCarStatusData pCarStatus, PacketSource packetSource)
        {
            Delay(pCarStatus.Header.SessionUID, pCarStatus.Header.SessionTime, packetSource);
        }

        public void OnPacketCarTelemetryData(ref PacketCarTelemetryData pTelemetry, PacketSource packetSource)
        {
            Delay(pTelemetry.Header.SessionUID, pTelemetry.Header.SessionTime, packetSource);
        }

        public void OnPacketLapData(ref PacketLapData pLap, PacketSource packetSource)
        {
            Delay(pLap.Header.SessionUID, pLap.Header.SessionTime, packetSource);
        }

        public void OnPacketMotionData(ref PacketMotionData pMotion, PacketSource packetSource)
        {
            Delay(pMotion.Header.SessionUID, pMotion.Header.SessionTime, packetSource);
        }

        public void OnPacketParticipantsData(ref PacketParticipantsData pParticipants, PacketSource packetSource)
        {
            Delay(pParticipants.Header.SessionUID, pParticipants.Header.SessionTime, packetSource);
        }

        private void Delay(ulong sessionId, float sessionTime, PacketSource packetSource)
        {
            if (packetSource != PacketSource.File || !_delayScale.Enabled) return;

            var lastSessionTime = _sessionLastTimes[sessionId];
            var diff = sessionTime - lastSessionTime;
            var scaledDiff = diff * _delayScale;
            Thread.Sleep(TimeSpan.FromSeconds(scaledDiff));
            _sessionLastTimes[sessionId] = sessionTime;
        }
    }
}
