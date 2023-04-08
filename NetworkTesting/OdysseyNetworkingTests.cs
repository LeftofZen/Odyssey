using Odyssey.Networking;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Testing
{
	public class OdysseyNetworkingTests
	{
		[Test]
		public void TestLogin()
		{
			var s = new OdysseyServer();
			s.Start();
			Assert.AreEqual(0, s.ClientCount);

			var c = new OdysseyClient(Constants.DefaultHostname, Constants.DefaultPort);
			c.Connect();

			var sw = new Stopwatch();
			sw.Restart();
			while (sw.Elapsed.TotalSeconds < 10)
			{ Thread.Sleep(100); }

			Assert.AreEqual(true, c.Connected);
			Assert.AreEqual(1, s.ClientCount);
		}
	}
}
