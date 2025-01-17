﻿using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using F1Telemetry.Core.Packets;
using F1TelemetryNetCore.Packets;
using log4net;

namespace F1Telemetry.Core
{
    public class PacketParser
    {
        private readonly IPacketHandler _packetHandler;
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ValidatePacketHeaderAndLength(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<PacketHeader>(buffer);
            if (buffer.Length < Unsafe.SizeOf<PacketHeader>())
            {
                Logger.Warn($"Invalid message size, could not read header. Size: {buffer.Length}");
                return false;
            }

            if (header.PacketFormat != 2018)
            {
                Logger.Warn($"Unsupported packet format: {header.PacketFormat}");
                return false;
            }

            if (header.PacketId < 0 || (int) header.PacketId >= PacketHeader.PacketSizes.Length)
            {
                Logger.Warn($"Unsupported packet ID: {header.PacketId}");
                return false;
            }

            var expectedPacketHeader = PacketHeader.PacketSizes[(int) header.PacketId];
            if (buffer.Length < expectedPacketHeader)
            {
                Logger.Warn( $"Size of the message does not match expected size. Actual: {buffer.Length}, Expected: {expectedPacketHeader}  for packet {((PacketHeader.PacketType) header.PacketId).ToString()}");
                return false;
            }

            Logger.DebugFormat("Received valid header {0}", header);
            return true;
        }

        public static unsafe bool TryReadFromBuffer<T>(ReadOnlySequence<byte> buffer, out T result, out int bytesRead) where T:struct
        {
            var packetSize = Unsafe.SizeOf<T>();
            if (buffer.Length < packetSize)
            {
                result = default;
                bytesRead = 0;
                return false;
            }

            if (buffer.FirstSpan.Length >= packetSize)
            {
                result = MemoryMarshal.Read<T>(buffer.FirstSpan.Slice(0, packetSize));
                bytesRead = packetSize;
                return true;
            }
            else
            {
                var slice = buffer.Slice(0, packetSize);
                Span<byte> tempBuffer = stackalloc byte[packetSize];
                slice.CopyTo(tempBuffer);

                result = MemoryMarshal.Read<T>(tempBuffer);
                bytesRead = packetSize;
                return true;
            }
        }

        public PacketParser(IPacketHandler packetHandler)
        {
            _packetHandler = packetHandler;
        }

        public async Task ReadMessages(PipeReader reader, PacketSource packetSource, CancellationToken ct = default)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var msg = await reader.ReadAsync(ct).ConfigureAwait(false);
                    if (msg.IsCompleted || msg.IsCanceled || ct.IsCancellationRequested)
                    {
                        break;
                    }

                    if (!TryReadFromBuffer(msg.Buffer, out PacketHeader header, out int headerBytesRead))
                    {
                        reader.AdvanceTo(msg.Buffer.Start, msg.Buffer.End);
                    }
                    else
                    {
                        switch (header.PacketId)
                        {
                            case PacketHeader.PacketType.Session:
                                if (HandleMessageAndAdvanceBuffer<PacketSessionData>(out var pSessionData))
                                {
                                    _packetHandler.OnPacketSessionData(ref pSessionData, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.CarSetups:
                                HandleMessageAndAdvanceBuffer<PacketCarSetupData>(out var pCarSetup);
                                {
                                    _packetHandler.OnPacketCarSetupData(ref pCarSetup, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.CarStatus:
                                HandleMessageAndAdvanceBuffer<PacketCarStatusData>(out var pCarStatus);
                                {
                                    _packetHandler.OnPacketCarStatusData(ref pCarStatus, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.CarTelemetry:
                                HandleMessageAndAdvanceBuffer<PacketCarTelemetryData>(out var pTelemetry);
                                {
                                    _packetHandler.OnPacketCarTelemetryData(ref pTelemetry, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.Event:
                                if (HandleMessageAndAdvanceBuffer<PacketEventData>(out var pEvent))
                                {
                                    _packetHandler.OnPacketEventData(ref pEvent, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.LapData:
                                HandleMessageAndAdvanceBuffer<PacketLapData>(out var pLap);
                                {
                                    _packetHandler.OnPacketLapData(ref pLap, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.Motion:
                                HandleMessageAndAdvanceBuffer<PacketMotionData>(out var pMotion);
                                {
                                    _packetHandler.OnPacketMotionData(ref pMotion, packetSource);
                                }
                                break;
                            case PacketHeader.PacketType.Participants:
                                HandleMessageAndAdvanceBuffer<PacketParticipantsData>(out var pParticipants);
                                {
                                    _packetHandler.OnPacketParticipantsData(ref pParticipants, packetSource);
                                }
                                break;
                            default:
                                throw new InvalidOperationException(
                                    $"Unsupported packet id {header.PacketId}, message should have been filtered before");
                        }
                    }

                    bool HandleMessageAndAdvanceBuffer<T>(out T sessionData) where T : struct
                    {
                        if (!TryReadFromBuffer<T>(msg.Buffer, out sessionData, out int bytesRead))
                        {
                            reader.AdvanceTo(msg.Buffer.Start, msg.Buffer.End);
                            return false;
                        }

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.DebugFormat("Successfully parsed {0}: {1}", typeof(T).Name, sessionData);
                        }

                        reader.AdvanceTo(msg.Buffer.GetPosition(bytesRead));
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception occured while parsing messages", ex);
                throw;
            }
            finally
            {
                reader?.Complete();
            }
        }
    }
}