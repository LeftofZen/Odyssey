namespace Odyssey.Networking
{
	public interface IMessageStreamDeserialiser<T>
	{
		T Deserialise(Header hdr, byte[] bytes);
	}

	public class ByteDeserialiser : IMessageStreamDeserialiser<byte[]>
	{
		public byte[] Deserialise(Header hdr, byte[] bytes)
			=> bytes;
	}

	public class MessageStreamReader<T>
	{
		private readonly BufferedStream bs;

		private readonly byte[] cbuf;
		private int ptrStart = 0;
		private int ptrEnd = 0; // exclusive

		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;
		public const int HeaderSize = 8;
		public Queue<(Header hdr, T msg)> DelimitedMessageQueue { get; init; } = new();

		private int DataAvailable => ptrEnd - ptrStart;

		private IMessageStreamDeserialiser<T> deserialiser;

		public MessageStreamReader(Stream stream, IMessageStreamDeserialiser<T> deserialiser, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
			cbuf = new byte[MaxMsgSize];
			this.deserialiser = deserialiser;
		}

		public void Update()
		{
			try
			{
				UpdateInternal();
			}
			catch (Exception)
			{
				//Log.Error(ex, "Couldn't read from stream");
			}
		}

		private void UpdateInternal()
		{
			// read from stream
			var read = bs.Read(cbuf, ptrEnd, 256);
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
					DelimitedMessageQueue.Enqueue((hdr, deserialiser.Deserialise(hdr, msgBytes.ToArray())));

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
