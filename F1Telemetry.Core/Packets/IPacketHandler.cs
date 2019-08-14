using System;
using System.Collections.Generic;
using System.Text;
using F1TelemetryNetCore.Packets;

namespace F1Telemetry.Core.Packets
{
    public interface IPacketHandler
    {
        void OnPacketEventData(ref PacketEventData pEventData);
        void OnStop();
        void OnPacketSessionData(ref PacketSessionData pSessionData);
    }
}
