using System.Runtime.InteropServices;

namespace Odyssey.Networking.Messages
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct MessageHeader : INetworkMessage
	{
		public NetworkMessageType Type { get; set; }

		// the length in bytes of the subsequent message (NOT including the header itself)
		public int Length { get; init; }
	}
}
