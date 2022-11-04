namespace Odyssey.Networking
{
	public class MessageStreamReader
	{
		private readonly BufferedStream bs;

		//private Queue<INetworkMessage> msgs = new();
		private Queue<(Header hdr, byte[] msg)> delimitedMsgs = new();

		private readonly byte[] cbuf;
		private int ptrStart = 0;
		private int ptrEnd = 0; // exclusive

		//private readonly Func<uint, Type> msgLookup;

		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;
		public const int HeaderSize = 8;
		//public Queue<INetworkMessage> MessageQueue => msgs;
		public Queue<(Header hdr, byte[] msg)> DelimitedMessageQueue => delimitedMsgs;

		private int DataAvailable => ptrEnd - ptrStart;

		public MessageStreamReader(Stream stream, Func<uint, Type> lookup = null, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
			cbuf = new byte[MaxMsgSize];
			//msgLookup = lookup;
		}

		public void Update()
		{
			//Log.Debug("[MessageStream::Update]");

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

					// internal deserialisation
					//var dmsg = (INetworkMessage)MessagePackSerializer.Deserialize(msgLookup(type), msgBytes);
					//msgs.Enqueue(dmsg);

					// external deserialisation
					var entry = (new Header() { Length = length, Type = type }, msgBytes.ToArray());
					delimitedMsgs.Enqueue(entry);

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
