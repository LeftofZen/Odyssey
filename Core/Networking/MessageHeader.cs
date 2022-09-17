using System.Runtime.InteropServices;

namespace Odyssey.Networking
{
	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct MessageHeader : INetworkMessage
	{
		public NetworkMessageType Type { get; set; }
	}
}
