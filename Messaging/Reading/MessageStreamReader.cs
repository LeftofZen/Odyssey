using Odyssey.Messaging;
using Serilog;

namespace Messaging.Reading
{
	public class MessageStreamReader<T>(Stream stream, IMessageStreamDeserialiser<T> deserialiser) 
		: ByteStreamReader(stream) where T : IMessage
	{
		private readonly IMessageStreamDeserialiser<T> deserialiser = deserialiser;

		public bool TryDequeue(out (Header hdr, T msg) dmsg)
		{
			Log.Verbose("[MessageStreamReader::TryDequeue]");

			if (DelimitedMessageQueue.TryDequeue(out var bmsg))
			{
				if (bmsg.hdr.Type == 0)
				{
					dmsg = default;
					return false;
				}

				dmsg = (bmsg.hdr, deserialiser.Deserialise(bmsg.hdr, bmsg.msg));
				return true;
			}

			dmsg = default;
			return false;
		}
	}
}
