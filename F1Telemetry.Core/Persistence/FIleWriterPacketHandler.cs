using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using F1Telemetry.Core.Packets;
using F1TelemetryNetCore.Packets;
using log4net;
using log4net.Repository.Hierarchy;

namespace F1Telemetry.Core.Persistence
{
    public sealed class FileWriterPacketHandler : IPacketHandler, IDisposable
    {
        private readonly Dictionary<ulong, Stream> _files = new Dictionary<ulong, Stream>();
        private readonly HashSet<ulong> _ignoredSessions = new HashSet<ulong>();

        private readonly string _saveLocation;
        private readonly string _filePrefix;

        private static ILog Logger =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public FileWriterPacketHandler(string saveLocation = null, string filePrefix = null)
        {
            _saveLocation =
                saveLocation ??  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "F1Telemetry");
            _filePrefix = filePrefix ?? "F1TelemetrySession";
        }

        public void OnPacketEventData(ref PacketEventData pEventData, PacketSource packetSource)
        {
/*
            if (packetSource == PacketSource.File)
            {
                return;
            }

*/
            var sessionId = pEventData.Header.SessionUID;
            if (pEventData.EventStringCode == "SSTA")
            {
                if (_files.TryGetValue(sessionId, out var stream))
                {
                    Logger.Warn(
                        $"Received session start for SessionId {sessionId} that already exists in active session list, will overwrite");
                    stream.Dispose();
                    _files.Remove(sessionId);
                }

                var path = Path.Combine(_saveLocation, $"{_filePrefix}_{sessionId}.f1s.gz");
                try
                {
                    if (!Directory.Exists(_saveLocation))
                    {
                        Directory.CreateDirectory(_saveLocation);
                    }

                    var fileStream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
                    var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
                    _files[sessionId] = gzipStream;
                    _ignoredSessions.Remove(sessionId);
                    stream = gzipStream;
                    Logger.Info($"Created save file for session {sessionId} at {path}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"Exception while creating a save file for session {sessionId} at {path}", ex);
                }

                WritePacket(sessionId, ref pEventData, packetSource);
            }
            else if (pEventData.EventStringCode == "SEND")
            {
                if (_files.TryGetValue(sessionId, out var stream))
                {
                    WritePacket(sessionId, ref pEventData, packetSource);

                    Logger.Info($"Closing save file for session {sessionId}");
                    stream.Dispose();
                    _files.Remove(sessionId);
                }
                else
                {
                    Logger.Warn($"Received session end for session {sessionId}, but no file is opened for saving");
                }
            }
        }

        private void WritePacket<T>(ulong sessionId, ref T packet, PacketSource source) where T : struct
        {
/*
            if (source == PacketSource.File)
            {
                return;
            }
*/

            if (_files.TryGetValue(sessionId, out var stream))
            {
                var readOnlySpan = MemoryMarshal.CreateReadOnlySpan(ref packet, 1);
                var onlySpan = MemoryMarshal.AsBytes(readOnlySpan);
                stream.Write(onlySpan);
            }
            else if(!_ignoredSessions.Contains(sessionId))
            {
                Logger.Warn( $"Ignoring this and further messages for session {sessionId} until session start event comes through and save file is created");
                _ignoredSessions.Add(sessionId);
            }
        }

        public void OnPacketSessionData(ref PacketSessionData pSessionData, PacketSource packetSource)
        {
            WritePacket(pSessionData.Header.SessionUID, ref pSessionData, packetSource);
        }

        public void OnPacketCarSetupData(ref PacketCarSetupData pCarSetup, PacketSource packetSource)
        {
            WritePacket(pCarSetup.Header.SessionUID, ref pCarSetup, packetSource);
        }

        public void OnPacketCarStatusData(ref PacketCarStatusData pCarStatus, PacketSource packetSource)
        {
            WritePacket(pCarStatus.Header.SessionUID, ref pCarStatus, packetSource);
        }

        public void OnPacketCarTelemetryData(ref PacketCarTelemetryData pTelemetry, PacketSource packetSource)
        {
            WritePacket(pTelemetry.Header.SessionUID, ref pTelemetry, packetSource);
        }

        public void OnPacketLapData(ref PacketLapData pLap, PacketSource packetSource)
        {
            WritePacket(pLap.Header.SessionUID, ref pLap, packetSource);
        }

        public void OnPacketMotionData(ref PacketMotionData pMotion, PacketSource packetSource)
        {
            WritePacket(pMotion.Header.SessionUID, ref pMotion, packetSource);
        }

        public void OnPacketParticipantsData(ref PacketParticipantsData pParticipants, PacketSource packetSource)
        {
            WritePacket(pParticipants.Header.SessionUID, ref pParticipants, packetSource);
        }

        public void Dispose()
        {
            foreach (var file in _files.Values)
            {
                file.Dispose();
            }
            _files.Clear();
        }
    }
}
