using Serilog;

namespace Odyssey.Networking
{
	public interface IMessageStreamSerialiser
	{
		byte[] Serialise<T>(T msg);
	}

	//public interface IMessageStreamSerialiser<T> : IMessageStreamSerialiser where T : struct, INetworkMessage
	//{
	//}

	public class ByteSerialiser : IMessageStreamSerialiser
	{
		public byte[] Serialise<T>(T msg)
			=> msg as byte[];
	}

	public class MessageStreamWriter<T>
	{
		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;

		private BufferedStream bs;

		private IMessageStreamSerialiser serialiser;

		public int PendingMessages { get; private set; } = 0;

		public MessageStreamWriter(Stream stream, IMessageStreamSerialiser serialiser, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
			this.serialiser = serialiser;
		}

		public void Enqueue<T>(T msg) where T : struct, INetworkMessage
		{
			Log.Debug("[MessageStreamWriter::Enqueue] {type}", msg.Type);

			var s = serialiser.Serialise<T>(msg);
			bs.Write(s);

			PendingMessages++;
		}

		public void EnqueueRaw(uint type, byte[] msg)
		{
			Log.Debug("[MessageStreamWriter::EnqueueRaw] {type}", type);

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
				Log.Debug("[MessageStreamWriter::Update] {pendingMessages}", PendingMessages);
				bs.Flush();
				PendingMessages = 0;
			}
		}
	}
}
