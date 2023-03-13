using MessagePack;

namespace Odyssey.Networking
{
	public class MessagePackSerialiser : IMessageStreamSerialiser<INetworkMessage>
	{
		public byte[] Serialise(INetworkMessage msg)
			=> MessagePackSerializer.Serialize(msg.GetType(), msg);
		public byte[] Serialise<T>(T msg) => throw new NotImplementedException();
	}
}
