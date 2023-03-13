using Serilog;

namespace Odyssey.Networking
{
	public interface IMessageStreamSerialiser<T> where T : INetworkMessage
	{
		byte[] Serialise(T msg);
	}

	public class MessageStreamWriter<T> : MessageStreamWriterBase where T : INetworkMessage
	{
		private IMessageStreamSerialiser<T> serialiser;

		public MessageStreamWriter(Stream stream, IMessageStreamSerialiser<T> serialiser, int maxMsgSize = DefaultMaxMsgSize) : base(stream, maxMsgSize) => this.serialiser = serialiser;

		public void Enqueue(T msg)
		{
			Log.Debug("[MessageStreamWriter::Enqueue] {type}", msg.Type);

			var s = serialiser.Serialise(msg);
			Enqueue(msg.Type, s);
		}
	}

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
