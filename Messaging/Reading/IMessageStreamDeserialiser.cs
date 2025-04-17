using Odyssey.Messaging;

namespace Messaging.Reading
{
	public interface IMessageStreamDeserialiser<T> where T : IMessage
	{
		T Deserialise(Header hdr, byte[] bytes);
	}
}
