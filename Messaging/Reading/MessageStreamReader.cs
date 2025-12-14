using Odyssey.Messaging;

namespace Messaging.Reading
{
	public class MessageStreamReader<T> : MessageStreamReaderBase where T : IMessage
	{
		private IMessageStreamDeserialiser<T> deserialiser;

		public bool TryDequeue(out (Header hdr, T msg) dmsg)
		{
			if (DelimitedMessageQueue.TryDequeue(out var bmsg))
			{
				dmsg = (bmsg.hdr, deserialiser.Deserialise(bmsg.hdr, bmsg.msg));
				return true;
			}

			dmsg = default;
			return false;
		}

		public MessageStreamReader(Stream stream, IMessageStreamDeserialiser<T> deserialiser, int maxMsgSize = 1024) 
			: base(stream, maxMsgSize) => this.deserialiser = deserialiser;
	}
}
