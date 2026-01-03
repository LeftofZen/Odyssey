using Odyssey.Messaging;
using Serilog;
using System.IO.Pipelines;

namespace Messaging.Writing
{
	public class ByteStreamWriter(Stream stream) : IDisposable
	{
		private readonly PipeWriter pipeWriter = PipeWriter.Create(stream, new StreamPipeWriterOptions(leaveOpen: true));
		private bool disposed = false;

		public int PendingMessages { get; private set; } = 0;

		public void Enqueue(uint type, byte[] msg)
		{
			if (disposed)
			{
				return;
			}

			Log.Debug("[ByteStreamWriter::Enqueue] {type}", type);

			// header
			var typeBytes = BitConverter.GetBytes(type);
			var lengthBytes = BitConverter.GetBytes(msg.Length);

			// Get memory from the pipe and write to it
			var span = pipeWriter.GetSpan(Constants.MessageHeaderSize + msg.Length);

			// Write header
			typeBytes.CopyTo(span);
			lengthBytes.CopyTo(span[4..]);

			// Write message
			msg.CopyTo(span[Constants.MessageHeaderSize..]);

			// Advance the writer
			pipeWriter.Advance(Constants.MessageHeaderSize + msg.Length);
			PendingMessages++;
		}

		public async Task UpdateAsync()
		{
			if (disposed)
			{
				return;
			}

			if (PendingMessages > 0)
			{
				Log.Debug("[ByteStreamWriter::Update] {pendingMessages}", PendingMessages);

				// Flush the pipe to send data
				await pipeWriter.FlushAsync();
				PendingMessages = 0;
			}
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
					// Flush any pending data before completing
					if (PendingMessages > 0)
					{
						UpdateAsync().RunSynchronously();
					}
					
					// Complete the PipeWriter to release any resources
					// This does not dispose the underlying stream as we specified leaveOpen: true
					pipeWriter.Complete();
				}

				disposed = true;
			}
		}
	}
}
