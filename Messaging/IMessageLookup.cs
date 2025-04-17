namespace Odyssey.Messaging
{
	// all messages in this library work like so
	// |-----------------|---------------------|
	// |     header      |         body        |
	// || type | length ||         body        |
	//
	// we always parse header first which is always 8 bytes
	// type is 4 bytes, representing the body message type, which is user defined
	// length is the length of the body only (not including header) in bytes

	public interface IMessageLookup<T> where T : struct, Enum
	{
		public IDictionary<uint, Type> ToType { get; }

		public IDictionary<Type, T> ToNetwork { get; }
	}
}
