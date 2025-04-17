using System.Runtime.InteropServices;

namespace Odyssey.Messaging
{
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
