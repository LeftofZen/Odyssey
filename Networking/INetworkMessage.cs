using System.Runtime.InteropServices;
using MessagePack;

namespace Odyssey.Networking
{
	public interface INetworkMessage
	{
		[IgnoreMember]
		bool RequiresLogin { get; }

		// the message type
		[IgnoreMember]
		uint Type { get; }
	}

	public interface IClientId
	{
		public Guid ClientId { get; init; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)] // this is only useful when we use messagepackstreams
	public struct Header
	{
		public uint Type { get; init; }
		public uint Length { get; set; }
	}
}
