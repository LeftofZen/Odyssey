using MessagePack;
using Messaging.Writing;
using Odyssey.Messaging;

namespace Messaging.MessagePack
{
	public class MessagePackSerialiser : IMessageStreamSerialiser<IMessage>
	{
		public byte[] Serialise(IMessage msg)
			=> MessagePackSerializer.Serialize(msg.GetType(), msg);
		public byte[] Serialise<T>(T msg) => throw new NotImplementedException();
	}
}
