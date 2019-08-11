using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using F1TelemetryNetCore.Packets;
using log4net;

namespace F1Telemetry.Core
{
    public class PacketParser
    {
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

        public async Task ReadMessages(PipeReader reader, CancellationToken ct)
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
                                ReadMessageAndAdvanceBuffer<PacketSessionData>();
                                break;
                            case PacketHeader.PacketType.CarSetups:
                                ReadMessageAndAdvanceBuffer<PacketCarSetupData>();
                                break;
                            case PacketHeader.PacketType.CarStatus:
                                ReadMessageAndAdvanceBuffer<PacketCarStatusData>();
                                break;
                            case PacketHeader.PacketType.CarTelemetry:
                                ReadMessageAndAdvanceBuffer<PacketCarTelemetryData>();
                                break;
                            case PacketHeader.PacketType.Event:
                                ReadMessageAndAdvanceBuffer<PacketEventData>();
                                break;
                            case PacketHeader.PacketType.LapData:
                                ReadMessageAndAdvanceBuffer<PacketLapData>();
                                break;
                            case PacketHeader.PacketType.Motion:
                                ReadMessageAndAdvanceBuffer<PacketMotionData>();
                                break;
                            case PacketHeader.PacketType.Participants:
                                ReadMessageAndAdvanceBuffer<PacketParticipantsData>();
                                break;
                            default:
                                throw new InvalidOperationException(
                                    $"Unsupported packet id {header.PacketId}, message should have been filtered before");
                        }
                    }

                    void ReadMessageAndAdvanceBuffer<T>() where T : struct
                    {
                        if (!TryReadFromBuffer<T>(msg.Buffer, out T sessionData, out int bytesRead))
                        {
                            reader.AdvanceTo(msg.Buffer.Start, msg.Buffer.End);
                            return;
                        }

                        if (Logger.IsDebugEnabled)
                        {
                            Logger.DebugFormat("Successfully parsed {0}: {1}", typeof(T).Name, sessionData);
                        }

                        reader.AdvanceTo(msg.Buffer.GetPosition(bytesRead));
                        return;
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