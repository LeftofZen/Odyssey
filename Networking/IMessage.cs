using System.Runtime.InteropServices;

namespace Odyssey.Networking
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

		IDictionary<uint, Type> toType { get; init; }

		IDictionary<Type, T> toNetwork { get; init; }

		public IDictionary<uint, Type> ToType
			=> toType;

		public IDictionary<Type, T> ToNetwork
			=> toNetwork;
	}

	public interface IMessage
	{
		bool RequiresLogin { get; }

		uint Type { get; }
	}

	public interface IClientId
	{
		public Guid ClientId { get; init; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	[Serializable]
	public struct Header
	{
		public uint Type { get; init; }
		public uint Length { get; set; }
	}
}
