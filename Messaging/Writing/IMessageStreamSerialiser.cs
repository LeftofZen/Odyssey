using Odyssey.Messaging;

namespace Messaging.Writing
{
	public interface IMessageStreamSerialiser<T> where T : IMessage
	{
		byte[] Serialise(T msg);
	}
}
