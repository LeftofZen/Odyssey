using Odyssey.Networking;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Testing
{
	public class ClientDisconnectTests
	{
		[SetUp]
		public void Setup()
		{
			// arrange
			const string addr = "127.0.0.1"; // localhost
			const int port = 54321;
			var endpoint = new IPEndPoint(IPAddress.Parse(addr), port);

			// start 'server'
			listener = new TcpListener(endpoint);
			listener.Start();
			var listenerTask = listener.AcceptTcpClientAsync();

			// start 'client'
			client = new TcpClient();
			client.Connect(endpoint);

			// wait till they're connected
			server = listenerTask.Result;

			Assert.Multiple(() =>
			{
				Assert.That(server.Connected, Is.True);
				Assert.That(client.Connected, Is.True);
			});
		}

		[TearDown]
		public void TearDown()
		{
			listener.Stop();

			client.Close();
			server.Close();

			client.Dispose();
			server.Dispose();
		}

		private TcpListener listener;
		private TcpClient server;
		private TcpClient client;

		[Test]
		public void TestDisconnect()
		{
			// assert client+server connected
			Assert.Multiple(() =>
			{
				Assert.True(server.Connected);
				Assert.True(client.Connected);
			});

			// disconnect client
			client.Close();

			// server should show client disconnected
			Assert.Multiple(() =>
			{
				Assert.False(server.Connected);
				Assert.False(client.Connected);
			});
		}

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
