using System.Net.Sockets;
using System.Runtime.InteropServices;
using Serilog;

namespace Odyssey.Networking
{
	public static class NetworkStreamExtensions
	{
		public static bool TryReadMessage<T>(this NetworkStream stream, out T tObj) where T : INetworkMessage
		{
			var bytes = new byte[Marshal.SizeOf(typeof(T))];
			var bytesRead = stream.Read(bytes, 0, bytes.Length);

			if (bytesRead != bytes.Length)
			{
				Log.Warning("unknown network message");
				tObj = default;
				return false;
			}

			tObj = Protocol.Deserialise<T>(bytes.AsSpan());
			return true;
		}
	}
}
