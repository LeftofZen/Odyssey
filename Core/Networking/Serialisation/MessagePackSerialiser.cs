using MessagePack;

namespace Odyssey.Networking
{
	public class MessagePackSerialiser : IMessageStreamSerialiser
	{
		public byte[] Serialise<T>(T msg) // where T : struct, INetworkMessage
		{
			var smsg = MessagePackSerializer.Serialize<T>(msg);

			var a = BitConverter.GetBytes((msg as INetworkMessage).Type);
			var b = BitConverter.GetBytes(smsg.Length);

			var cbuf = new byte[8 + smsg.Length];
			Array.Copy(a, 0, cbuf, 0, 4);
			Array.Copy(b, 0, cbuf, 4, 4);
			Array.Copy(smsg, 0, cbuf, 8, smsg.Length);

			return cbuf;
		}
	}
}
