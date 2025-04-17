using System.Net;
using System.Net.Sockets;

namespace Network
{
	[TestFixture]
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
			listener.Dispose();
		}

		private TcpListener listener;
		private TcpClient server;
		private TcpClient client;

		[Test]
		public void Disconnect()
		{
			// assert client+server connected
			Assert.Multiple(() =>
			{
				Assert.That(server.Connected);
				Assert.That(client.Connected);
			});

			// disconnect client
			client.Close();

			// server should show client disconnected
			Assert.Multiple(() =>
			{
				Assert.That(server.Connected);
				Assert.That(client.Connected);
			});
		}
	}
}
