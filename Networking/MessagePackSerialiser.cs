using MessagePack;

namespace Odyssey.Networking
{
	public class MessagePackSerialiser : IMessageStreamSerialiser<IMessage>
	{
		public byte[] Serialise(IMessage msg)
			=> MessagePackSerializer.Serialize(msg.GetType(), msg);
		public byte[] Serialise<T>(T msg) => throw new NotImplementedException();
	}
}
