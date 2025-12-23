using Odyssey.Messaging;
using Serilog;
using System.Diagnostics;

namespace Messaging.Reading
{

	public class MessageStreamReaderBase
	{
		private readonly BufferedStream bs;

		private readonly byte[] cbuf;
		public int ptrStart = 0;
		public int ptrEnd = 0; // exclusive

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

			// FIRST: Compact the buffer if there's unconsumed data not at the start
			if (ptrStart > 0 && DataAvailable > 0)
			{
				Array.ConstrainedCopy(cbuf, ptrStart, cbuf, 0, DataAvailable);
				ptrEnd = DataAvailable;
				ptrStart = 0;
			}
			else if (ptrStart > 0)
			{
				// No data left, reset pointers
				ptrStart = 0;
				ptrEnd = 0;
			}

			// SECOND: Read new data from stream into available buffer space
			int spaceAvailable = cbuf.Length - ptrEnd;
			if (spaceAvailable > 0)
			{
				var read = bs.Read(cbuf, ptrEnd, Math.Min(spaceAvailable, 256));
				ptrEnd += read;
				
				if (read > 0)
				{
					Log.Verbose("[MessageStreamReaderBase::UpdateInternal] Read {bytes} bytes from stream, buffer now has {available} bytes", read, DataAvailable);
				}
			}
			else
			{
				Log.Warning("[MessageStreamReaderBase::UpdateInternal] Buffer full with no space to read more data");
			}

			var rom = new ReadOnlyMemory<byte>(cbuf);

			// THIRD: Process buffer into as many complete messages as possible
			while (true)
			{
				if (DataAvailable >= HeaderSize)
				{
					// read header
					var type = BitConverter.ToUInt32(rom.Slice(ptrStart, 4).Span);

					if (type == 0)
					{
						// weird empty data?
						ptrStart += 4;
						break;
					}

					var length = BitConverter.ToUInt32(rom.Slice(ptrStart + 4, 4).Span);
					if (length == 0)
					{
						// weird empty data?
						ptrStart += 4;
						break;
					}

					if (DataAvailable >= HeaderSize + length)
					{
						var msgBytes = rom.Slice(ptrStart + HeaderSize, (int)length);

						if (type == 0 || length == 0)
						{
							Debugger.Break();
							break;
						}

						// external deserialisation
						var hdr = new Header() { Type = type, Length = length };

						if (DelimitedMessageQueue.Count > 100)
						{
							Log.Warning("MessageStreamReaderBase::UpdateInternal - DelimitedMessageQueue size is {Count}, possible message processing lag", DelimitedMessageQueue.Count);
							Debugger.Break();
						}

						DelimitedMessageQueue.Enqueue((hdr, msgBytes.ToArray()));
						ptrStart += HeaderSize + (int)length;
						Log.Verbose("[MessageStreamReaderBase::UpdateInternal] Enqueued message type={type} length={length}", type, length);
					}
					else
					{
						// Not enough data for complete message, wait for more
						Log.Verbose("[MessageStreamReaderBase::UpdateInternal] Incomplete message: need {needed} bytes, have {available}", HeaderSize + length, DataAvailable);
						break;
					}
				}
				else
				{
					// Not enough data for header, wait for more
					break;
				}
			}

			// Note: We don't copy remaining data here anymore - we do it at the start of next Update()
		}
	}
}
