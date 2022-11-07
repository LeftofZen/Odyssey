using MessagePack;

namespace Odyssey.Networking
{
	public class MessagePackDeserialiser : IMessageStreamDeserialiser<INetworkMessage>
	{
		public INetworkMessage Deserialise(Header hdr, byte[] bytes)
			=> (INetworkMessage)MessagePackSerializer.Deserialize(MessageLookup.ToType[(NetworkMessageType)hdr.Type], bytes);
	}
}
