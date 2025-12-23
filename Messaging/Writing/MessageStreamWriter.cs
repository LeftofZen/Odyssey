using Odyssey.Messaging;
using Serilog;

namespace Messaging.Writing
{

	public class MessageStreamWriter<T> : MessageStreamWriterBase where T : IMessage
	{
		private IMessageStreamSerialiser<T> serialiser;

		public MessageStreamWriter(Stream stream, IMessageStreamSerialiser<T> serialiser, int maxMsgSize = DefaultMaxMsgSize) 
			: base(stream, maxMsgSize) => this.serialiser = serialiser;

		public void Enqueue(T msg)
		{
			Log.Debug("[MessageStreamWriter::Enqueue] {type}", msg.Type);

			var s = serialiser.Serialise(msg);
			Enqueue(msg.Type, s);
		}
	}
}
