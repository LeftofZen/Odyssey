using Odyssey.Messaging;
using Serilog;

namespace Messaging.Writing
{
	public class MessageStreamWriter<T>(Stream stream, IMessageStreamSerialiser<T> serialiser) 
		: ByteStreamWriter(stream) where T : IMessage
	{
		private readonly IMessageStreamSerialiser<T> serialiser = serialiser;

		public void Enqueue(T msg)
		{
			Log.Verbose("[MessageStreamWriter::Enqueue] {type}", msg.Type);

			var s = serialiser.Serialise(msg);
			Enqueue(msg.Type, s);
		}
	}
}
