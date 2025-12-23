using Serilog;

namespace Messaging.Writing
{
	public class MessageStreamWriterBase
	{
		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;

		private BufferedStream bs;

		public int PendingMessages { get; private set; } = 0;

		public MessageStreamWriterBase(Stream stream, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
		}

		public void Enqueue(uint type, byte[] msg)
		{
			Log.Debug("[MessageStreamWriterBase::Enqueue] {type}", type);

			var a = BitConverter.GetBytes(type);
			var b = BitConverter.GetBytes(msg.Length);

			if (!bs.CanWrite)
			{
				Log.Error($"[MessageStreamWriterBase::Enqueue] Stream cannot write. Message {type} lost");
				return;
			}

			bs.Write(a);
			bs.Write(b);
			bs.Write(msg);

			PendingMessages++;
		}

		public void Update()
		{
			if (PendingMessages > 0)
			{
				Log.Debug("[MessageStreamWriterBase::Update] {pendingMessages}", PendingMessages);
				bs.Flush();
				PendingMessages = 0;
			}
		}
	}
}
