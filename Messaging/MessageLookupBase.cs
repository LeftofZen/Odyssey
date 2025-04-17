namespace Odyssey.Messaging
{
	public class MessageLookupBase<T> : IMessageLookup<T> where T : struct, Enum
	{
		public unsafe uint GetUnderlyingValue(T value)
		{
			var tr = __makeref(value);
			var ptr = **(IntPtr**)&tr;
			return (uint)*(int*)ptr;
		}

		public MessageLookupBase(IDictionary<T, Type> toType, IDictionary<Type, T> toNetwork)
		{
			var underlyingType = Enum.GetUnderlyingType(typeof(T));
			if (underlyingType != typeof(int) && underlyingType != typeof(uint))
			{
				throw new ArgumentException("Message type enum didn't have underlying type of uint", nameof(toType));
			}

			this.toType = toType.ToDictionary(kvp => GetUnderlyingValue(kvp.Key), kvp => kvp.Value); // Convert is slow and does boxing, but this code is only run once so no need to optimise with pointers
			this.toNetwork = toNetwork;
		}

		private IDictionary<uint, Type> toType { get; init; }

		private IDictionary<Type, T> toNetwork { get; init; }

		public IDictionary<uint, Type> ToType
			=> toType;

		public IDictionary<Type, T> ToNetwork
			=> toNetwork;
	}
}
