using MessagePack;

namespace Odyssey.Networking
{
	public class MessagePackDeserialiser<TEnum> : IMessageStreamDeserialiser<IMessage> where TEnum : struct, Enum
	{
		// you give deserialiser a message definition in IMessageLookup and it'll automatically convert your
		// messages to the correct type
		public MessagePackDeserialiser(IMessageLookup<TEnum> lookup)
			=> Lookup = lookup;

		public IMessageLookup<TEnum> Lookup { get; }

		public IMessage Deserialise(Header hdr, byte[] bytes)
			=> (IMessage)MessagePackSerializer.Deserialize(Lookup.ToType[hdr.Type], bytes);
	}
}
