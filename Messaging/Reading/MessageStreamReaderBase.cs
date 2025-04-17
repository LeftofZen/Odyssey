using Odyssey.Messaging;
using Serilog;

namespace Messaging.Reading
{

	public class MessageStreamReaderBase
	{
		private readonly BufferedStream bs;

		private readonly byte[] cbuf;
		private int ptrStart = 0;
		private int ptrEnd = 0; // exclusive

		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;
		public const int HeaderSize = 8;
		public Queue<(Header hdr, byte[] msg)> DelimitedMessageQueue { get; init; } = new();

		private int DataAvailable => ptrEnd - ptrStart;

		public MessageStreamReaderBase(Stream stream, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
			cbuf = new byte[MaxMsgSize];
		}

		public void Update()
		{
			try
			{
				UpdateInternal();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Couldn't read from stream");
			}
		}

		private void UpdateInternal()
		{
			// read from stream
			var read = bs.Read(cbuf, ptrEnd, Math.Min(cbuf.Length - ptrEnd, 256));
			ptrEnd += read;

			var rom = new ReadOnlyMemory<byte>(cbuf);

			// process buffer into as many messages as possible
			while (DataAvailable >= HeaderSize)
			{
				// read header
				var type = BitConverter.ToUInt32(rom.Slice(ptrStart, 4).Span);
				var length = BitConverter.ToUInt32(rom.Slice(ptrStart + 4, 4).Span);

				if (DataAvailable >= HeaderSize + length)
				{
					var msgBytes = rom.Slice(ptrStart + HeaderSize, (int)length);

					// external deserialisation
					var hdr = new Header() { Type = type, Length = length };
					DelimitedMessageQueue.Enqueue((hdr, msgBytes.ToArray()));

					ptrStart += HeaderSize + (int)length;
				}
			}

			// copy any remaining data back to start of buffer
			if (DataAvailable > 0)
			{
				Array.ConstrainedCopy(cbuf, ptrStart, cbuf, 0, DataAvailable);
				ptrStart = 0;
				ptrEnd = DataAvailable; // aka 0 + available

				// we should zero out the rest of the buffer if we care about security more than performance
				// (or just use double buffering and zero it out in another thread/task)
			}
		}
	}
}
