using Serilog;
using System.IO.Pipelines;

namespace Messaging.Writing
{
	public class MessageStreamWriterBase
	{
		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;

		private PipeWriter pipeWriter;

		public int PendingMessages { get; private set; } = 0;

		public MessageStreamWriterBase(Stream stream, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			pipeWriter = PipeWriter.Create(stream);
		}

		public void Enqueue(uint type, byte[] msg)
		{
			Log.Debug("[MessageStreamWriterBase::Enqueue] {type}", type);

			var typeBytes = BitConverter.GetBytes(type);
			var lengthBytes = BitConverter.GetBytes(msg.Length);

			// Get memory from the pipe and write to it
			var memory = pipeWriter.GetMemory(8 + msg.Length);
			var span = memory.Span;

			// Write header
			typeBytes.CopyTo(span);
			lengthBytes.CopyTo(span.Slice(4));

			// Write message
			msg.CopyTo(span.Slice(8));

			// Advance the writer
			pipeWriter.Advance(8 + msg.Length);

			PendingMessages++;
		}

		public void Update()
		{
			if (PendingMessages > 0)
			{
				Log.Debug("[MessageStreamWriterBase::Update] {pendingMessages}", PendingMessages);
				
				// Flush the pipe to send data
				var flushResult = pipeWriter.FlushAsync().AsTask().Result;
				
				if (flushResult.IsCompleted)
				{
					Log.Information("[MessageStreamWriterBase::Update] PipeWriter completed");
				}
				
				PendingMessages = 0;
			}
		}
	}
}
