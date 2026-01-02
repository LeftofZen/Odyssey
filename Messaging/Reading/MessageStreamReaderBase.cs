using Odyssey.Messaging;
using Serilog;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;

namespace Messaging.Reading
{

	public class MessageStreamReaderBase : IDisposable
	{
		private readonly PipeReader pipeReader;
		private bool disposed = false;

		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;
		public const int HeaderSize = 8;
		public Queue<(Header hdr, byte[] msg)> DelimitedMessageQueue { get; init; } = new();

		public MessageStreamReaderBase(Stream stream, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			pipeReader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: true));
		}

		public void Update()
		{
			try
			{
				Log.Verbose("[MessageStreamReaderBase::Update]");
				UpdateInternal();
				Log.Verbose("[MessageStreamReaderBase::UpdateInternal] end");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Couldn't read from stream");
			}
		}

		private void UpdateInternal()
		{
			Log.Verbose("[MessageStreamReaderBase::UpdateInternal]");

			// Read from the pipe
			var readResult = pipeReader.ReadAsync().AsTask().Result;
			var buffer = readResult.Buffer;

			Log.Verbose("[MessageStreamReaderBase::UpdateInternal] Read buffer with {bytes} bytes", buffer.Length);

			// Process all complete messages in the buffer
			SequencePosition consumed = buffer.Start;
			SequencePosition examined = buffer.End;

			while (TryReadMessage(buffer, out var position, out var header, out var messageBytes))
			{
				if (header.Type == 0 || header.Length == 0)
				{
					Debugger.Break();
					break;
				}

				if (DelimitedMessageQueue.Count > 100)
				{
					Log.Warning("MessageStreamReaderBase::UpdateInternal - DelimitedMessageQueue size is {Count}, possible message processing lag", DelimitedMessageQueue.Count);
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

			if (readResult.IsCompleted)
			{
				Log.Information("[MessageStreamReaderBase::UpdateInternal] PipeReader completed");
			}
		}

		private bool TryReadMessage(ReadOnlySequence<byte> buffer, out SequencePosition position, out Header header, out byte[] messageBytes)
		{
			position = buffer.Start;
			header = default;
			messageBytes = Array.Empty<byte>();

			// Need at least header size
			if (buffer.Length < HeaderSize)
			{
				Log.Verbose("[MessageStreamReaderBase::TryReadMessage] Not enough data for header: have {available}", buffer.Length);
				return false;
			}

			// Read header
			Span<byte> headerBytes = stackalloc byte[HeaderSize];
			buffer.Slice(0, HeaderSize).CopyTo(headerBytes);

			var type = BitConverter.ToUInt32(headerBytes.Slice(0, 4));
			var length = BitConverter.ToUInt32(headerBytes.Slice(4, 4));

			if (type == 0 || length == 0)
			{
				// Skip invalid data
				Log.Verbose("[MessageStreamReaderBase::TryReadMessage] Invalid header: type={type} length={length}", type, length);
				return false;
			}

			// Check if we have the full message
			var totalMessageSize = HeaderSize + (int)length;
			if (buffer.Length < totalMessageSize)
			{
				Log.Verbose("[MessageStreamReaderBase::TryReadMessage] Incomplete message: need {needed} bytes, have {available}", totalMessageSize, buffer.Length);
				return false;
			}

			// Extract the message bytes
			messageBytes = buffer.Slice(HeaderSize, (int)length).ToArray();
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
