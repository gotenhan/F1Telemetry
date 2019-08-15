using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using F1TelemetryNetCore.Packets;
using log4net;
using log4net.Repository.Hierarchy;

namespace F1Telemetry.Core.Packets
{
    public class CompositePacketHandler: IPacketHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPacketHandler[] _packetHandlers;
        public CompositePacketHandler(params IPacketHandler[] packetHandlers)
        {
            _packetHandlers = packetHandlers;
        }

        public void OnPacketEventData(ref PacketEventData pEventData, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketEventData, ref pEventData, packetSource);
        }

        public void OnPacketSessionData(ref PacketSessionData pSessionData, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketSessionData, ref pSessionData, packetSource);
        }

        public void OnPacketCarSetupData(ref PacketCarSetupData pCarSetup, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketCarSetupData, ref pCarSetup, packetSource);
        }

        public void OnPacketCarStatusData(ref PacketCarStatusData pCarStatus, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketCarStatusData, ref pCarStatus, packetSource);

        }

        public void OnPacketCarTelemetryData(ref PacketCarTelemetryData pTelemetry, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketCarTelemetryData, ref pTelemetry, packetSource);

        }

        public void OnPacketLapData(ref PacketLapData pLap, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketLapData, ref pLap, packetSource);

        }

        public void OnPacketMotionData(ref PacketMotionData pMotion, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketMotionData, ref pMotion, packetSource);

        }

        public void OnPacketParticipantsData(ref PacketParticipantsData pParticipants, PacketSource packetSource)
        {
            HandlePacket(ph => ph.OnPacketParticipantsData, ref pParticipants, packetSource);
        }

        private delegate void PacketHandler<T>(ref T packet, PacketSource packetSource);

        private void HandlePacket<T>(Func<IPacketHandler, PacketHandler<T>> handler, ref T packet, PacketSource packetSource)
        {
            foreach (var ph in _packetHandlers)
            {
                try
                {
                    handler(ph)(ref packet, packetSource);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception occured while calling one of the handlers", ex);
                }
            }
        }
    }
}
