using MessagePack;

namespace Odyssey.Networking
{
	public class MessageStreamWriter
	{
		public int MaxMsgSize { get; init; }
		public const int DefaultMaxMsgSize = 1024;

		private BufferedStream bs;

		public MessageStreamWriter(Stream stream, int maxMsgSize = DefaultMaxMsgSize)
		{
			MaxMsgSize = maxMsgSize;
			bs = new BufferedStream(stream, MaxMsgSize);
		}

		public void Enqueue<T>(T msg) where T : struct, INetworkMessage
		{
			var s = Serialise<T>(msg);
			bs.Write(s);
		}

		public void Enqueue(uint type, byte[] msg)
		{
			var a = BitConverter.GetBytes(type);
			var b = BitConverter.GetBytes(msg.Length);

			bs.Write(a);
			bs.Write(b);
			bs.Write(msg);
		}

		public void Flush() => bs.Flush();

		private byte[] Serialise<T>(T msg) where T : struct, INetworkMessage
		{
			var smsg = MessagePackSerializer.Serialize<T>(msg);

			var a = BitConverter.GetBytes(smsg.Length);
			var b = BitConverter.GetBytes(msg.Type);

			var cbuf = new byte[8 + smsg.Length];
			Array.Copy(a, 0, cbuf, 0, 4);
			Array.Copy(b, 0, cbuf, 4, 4);
			Array.Copy(smsg, 0, cbuf, 8, smsg.Length);

			return cbuf;
		}
	}
}
