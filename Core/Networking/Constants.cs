using System.Net;

namespace Odyssey.Networking
{
	public static class Constants
	{
		public const int MessageHeaderSize = 8;
		public const int NetworkInputSize = 128;

		public static IPAddress DefaultHostname = IPAddress.Parse("127.0.0.1");
		public const int DefaultPort = 13002;
	}
}
