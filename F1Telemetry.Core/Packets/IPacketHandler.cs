using System;
using System.Collections.Generic;
using System.Text;
using F1TelemetryNetCore.Packets;

namespace F1Telemetry.Core.Packets
{
    public interface IPacketHandler
    {
        void OnPacketEventData(ref PacketEventData pEventData, PacketSource packetSource);
        void OnPacketSessionData(ref PacketSessionData pSessionData, PacketSource packetSource);
        void OnPacketCarSetupData(ref PacketCarSetupData pCarSetup, PacketSource packetSource);
        void OnPacketCarStatusData(ref PacketCarStatusData pCarStatus, PacketSource packetSource);
        void OnPacketCarTelemetryData(ref PacketCarTelemetryData pTelemetry, PacketSource packetSource);
        void OnPacketLapData(ref PacketLapData pLap, PacketSource packetSource);
        void OnPacketMotionData(ref PacketMotionData pMotion, PacketSource packetSource);
        void OnPacketParticipantsData(ref PacketParticipantsData pParticipants, PacketSource packetSource);
    }
}
