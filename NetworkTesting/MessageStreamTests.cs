using System.Net;
using System.Net.Sockets;
using Odyssey.Networking;

namespace NetworkTesting
{
	public class MessageStreamTests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TestMessageStream_Simplex()
		{
			// arrange
			const string addr = "127.0.0.1"; // localhost
			const int port = 54321;
			var endpoint = new IPEndPoint(IPAddress.Parse(addr), port);

			// start 'server'
			var listener = new TcpListener(endpoint);
			listener.Start();
			var listenerTask = listener.AcceptTcpClientAsync();

			// start 'client'
			var client = new TcpClient();
			client.Connect(endpoint);

			// wait till they're connected
			var server = listenerTask.Result;

			Assert.Multiple(() =>
			{
				Assert.That(server.Connected, Is.True);
				Assert.That(client.Connected, Is.True);
			});

			var writer = new MessageStreamWriter(server.GetStream());
			var reader = new MessageStreamReader(client.GetStream());

			// act - write a message from server to client
			const uint type = 16;
			var msg = new byte[] { 0, 1, 2, 5 };
			writer.Enqueue(type, msg);
			writer.Flush();

			// wait some time, could do async here
			Thread.Sleep(100);

			reader.Update();

			// assert
			var dmsg = reader.DelimitedMessageQueue.Peek();

			Assert.Multiple(() =>
			{
				Assert.That(dmsg.hdr.Type, Is.EqualTo(16));
				Assert.That(dmsg.hdr, Has.Length.EqualTo(4));

				Assert.That(dmsg.msg[0], Is.EqualTo(0));
				Assert.That(dmsg.msg[1], Is.EqualTo(1));
				Assert.That(dmsg.msg[2], Is.EqualTo(2));
				Assert.That(dmsg.msg[3], Is.EqualTo(5));
			});
		}

		[Test]
		public void TestMessageStream_Duplex()
		{
			// arrange
			const string addr = "127.0.0.1"; // localhost
			const int port = 54322;
			var endpoint = new IPEndPoint(IPAddress.Parse(addr), port);

			// start 'server'
			var listener = new TcpListener(endpoint);
			listener.Start();
			var listenerTask = listener.AcceptTcpClientAsync();

			// start 'client'
			var client = new TcpClient();
			client.Connect(endpoint);

			// wait till they're connected
			var server = listenerTask.Result;

			Assert.Multiple(() =>
			{
				Assert.That(server.Connected, Is.True);
				Assert.That(client.Connected, Is.True);
			});

			var serverWriter = new MessageStreamWriter(server.GetStream());
			var serverReader = new MessageStreamReader(server.GetStream());
			var clientWriter = new MessageStreamWriter(client.GetStream());
			var clientReader = new MessageStreamReader(client.GetStream());

			// act - write on server
			{
				const uint type = 16;
				var msg = new byte[] { 0, 1, 2, 5 };
				serverWriter.Enqueue(type, msg);
				serverWriter.Flush();
			}

			// act - write on client
			{
				const uint type = 7;
				var msg = new byte[] { 37, 14, 57, 75, 22 };
				clientWriter.Enqueue(type, msg);
				clientWriter.Flush();
			}

			// wait some time, could do async here
			Thread.Sleep(100);

			serverReader.Update();
			clientReader.Update();

			// assert
			var clientDmsg = clientReader.DelimitedMessageQueue.Peek();
			var serverDmsg = serverReader.DelimitedMessageQueue.Peek();

			Assert.Multiple(() =>
			{
				Assert.That(clientDmsg.hdr.Type, Is.EqualTo(16));
				Assert.That(clientDmsg.hdr, Has.Length.EqualTo(4));

				Assert.That(clientDmsg.msg[0], Is.EqualTo(0));
				Assert.That(clientDmsg.msg[1], Is.EqualTo(1));
				Assert.That(clientDmsg.msg[2], Is.EqualTo(2));
				Assert.That(clientDmsg.msg[3], Is.EqualTo(5));
			});

			Assert.Multiple(() =>
			{
				Assert.That(serverDmsg.hdr.Type, Is.EqualTo(7));
				Assert.That(serverDmsg.hdr, Has.Length.EqualTo(5));

				Assert.That(serverDmsg.msg[0], Is.EqualTo(37));
				Assert.That(serverDmsg.msg[1], Is.EqualTo(14));
				Assert.That(serverDmsg.msg[2], Is.EqualTo(57));
				Assert.That(serverDmsg.msg[3], Is.EqualTo(75));
				Assert.That(serverDmsg.msg[4], Is.EqualTo(22));
			});
		}
	}
}