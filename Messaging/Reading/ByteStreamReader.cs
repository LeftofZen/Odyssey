using Odyssey.Messaging;
using Serilog;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;

namespace Messaging.Reading
{
	public class ByteStreamReader(Stream stream) : IDisposable
	{
		private readonly PipeReader pipeReader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
		private bool disposed = false;

		public Queue<(Header hdr, byte[] msg)> DelimitedMessageQueue { get; init; } = new();

		public void Update()
		{
			if (disposed)
			{
				return;
			}

			Log.Verbose("[ByteStreamReader::Update]");
			
			try
			{
				UpdateInternal();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "[ByteStreamReader::Update] Couldn't read from stream");
			}
		}

		private void UpdateInternal()
		{
			Log.Verbose("[ByteStreamReader::UpdateInternal]");

			// Read from the pipe
			// Note: Using GetAwaiter().GetResult() to maintain synchronous API contract while avoiding .Result deadlock risks
			var readResult = pipeReader.ReadAsync().AsTask().GetAwaiter().GetResult();
			var buffer = readResult.Buffer;

			Log.Verbose("[ByteStreamReader::UpdateInternal] Read buffer with {bytes} bytes", buffer.Length);

			// Process all complete messages in the buffer
			var consumed = buffer.Start;
			var examined = buffer.End;

			while (TryReadMessage(buffer, out var position, out var header, out var messageBytes))
			{
				if (header.Type == 0 || header.Length == 0)
				{
					Debugger.Break();
					break;
				}

				if (DelimitedMessageQueue.Count > 100)
				{
					Log.Warning("ByteStreamReader::UpdateInternal - DelimitedMessageQueue size is {Count}, possible message processing lag", DelimitedMessageQueue.Count);
					Debugger.Break();
				}

				DelimitedMessageQueue.Enqueue((header, messageBytes));
				Log.Verbose("[MessageStreamReaderBase::UpdateInternal] Enqueued message type={type} length={length}", header.Type, header.Length);

				// Advance the buffer past the consumed message
				buffer = buffer.Slice(position);
				consumed = position;
			}

			// Tell the PipeReader how much of the buffer we consumed
			pipeReader.AdvanceTo(consumed, examined);
		}

		private bool TryReadMessage(ReadOnlySequence<byte> buffer, out SequencePosition position, out Header header, out byte[] messageBytes)
		{
			position = buffer.Start;
			header = default;
			messageBytes = [];

			// Need at least header size
			if (buffer.Length < Constants.DefaultMaxMessageSize)
			{
				Log.Verbose("[ByteStreamReader::TryReadMessage] Not enough data for header: have {available}", buffer.Length);
				return false;
			}

			// Read header
			Span<byte> headerBytes = stackalloc byte[Constants.DefaultMaxMessageSize];
			buffer.Slice(0, Constants.DefaultMaxMessageSize).CopyTo(headerBytes);

			var type = BitConverter.ToUInt32(headerBytes[..4]);
			var length = BitConverter.ToUInt32(headerBytes.Slice(4, 4));

			if (type == 0 || length == 0)
			{
				// Skip invalid data
				Log.Verbose("[ByteStreamReader::TryReadMessage] Invalid header: type={type} length={length}", type, length);
				return false;
			}

			// Check if we have the full message
			var totalMessageSize = Constants.DefaultMaxMessageSize + (int)length;
			if (buffer.Length < totalMessageSize)
			{
				Log.Verbose("[ByteStreamReader::TryReadMessage] Incomplete message: need {needed} bytes, have {available}", totalMessageSize, buffer.Length);
				return false;
			}

			// Extract the message bytes
			messageBytes = buffer.Slice(Constants.DefaultMaxMessageSize, (int)length).ToArray();
			header = new Header() { Type = type, Length = length };

			// Return the position after this message
			position = buffer.GetPosition(totalMessageSize);
			return true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					// Complete the PipeReader to release any resources
					// This does not dispose the underlying stream as we specified leaveOpen: true
					pipeReader.Complete();
				}

				disposed = true;
			}
		}
	}
}
