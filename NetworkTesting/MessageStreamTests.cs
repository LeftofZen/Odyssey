using System.Net;
using System.Net.Sockets;
using Odyssey.Networking;

namespace NetworkTesting
{
	[NonParallelizable] // all these tests use the same client/server endpoints, which cannot be used concurrently
	public class MessageStreamTests
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
		public void TestMessageStream_Simplex_Single()
		{
			// arrange
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
				Assert.That(dmsg.hdr.Length, Is.EqualTo(4));
				CollectionAssert.AreEqual(msg, dmsg.msg);
			});
		}

		[Test]
		public void TestMessageStream_Simplex_Multiple()
		{
			// arrange
			var writer = new MessageStreamWriter(server.GetStream());
			var reader = new MessageStreamReader(client.GetStream());

			// act - write a message from server to client
			const uint type1 = 16;
			var msg1 = new byte[] { 0, 1, 2, 5 };
			writer.Enqueue(type1, msg1);

			const uint type2 = 1;
			var msg2 = new byte[] { 19, 23, 45, 57, 53, 78, 97 };
			writer.Enqueue(type2, msg2);

			const uint type3 = 158;
			var msg3 = new byte[] { 9, 8, 6, 4, 3 };
			writer.Enqueue(type3, msg3);

			writer.Flush();
			Thread.Sleep(100);
			reader.Update(); // don't need this update, but its good to test updating in the middle of the stream

			const uint type4 = 63;
			var msg4 = new byte[] { 1, 1, 1, 2, 1, 1, 1, 99 };
			writer.Enqueue(type4, msg4);

			writer.Flush();
			Thread.Sleep(100);
			reader.Update();

			// assert
			var msgs = reader.DelimitedMessageQueue.ToList();
			Assert.Multiple(() =>
			{
				Assert.That(msgs[0].hdr.Type, Is.EqualTo(16));
				Assert.That(msgs[0].hdr.Length, Is.EqualTo(4));
				CollectionAssert.AreEqual(msg1, msgs[0].msg);

				Assert.That(msgs[1].hdr.Type, Is.EqualTo(1));
				Assert.That(msgs[1].hdr.Length, Is.EqualTo(7));
				CollectionAssert.AreEqual(msg2, msgs[1].msg);

				Assert.That(msgs[2].hdr.Type, Is.EqualTo(158));
				Assert.That(msgs[2].hdr.Length, Is.EqualTo(5));
				CollectionAssert.AreEqual(msg3, msgs[2].msg);

				Assert.That(msgs[3].hdr.Type, Is.EqualTo(63));
				Assert.That(msgs[3].hdr.Length, Is.EqualTo(8));
				CollectionAssert.AreEqual(msg4, msgs[3].msg);
			});
		}

		[Test]
		public void TestMessageStream_Duplex()
		{
			// arrange
			var serverWriter = new MessageStreamWriter(server.GetStream());
			var serverReader = new MessageStreamReader(server.GetStream());
			var clientWriter = new MessageStreamWriter(client.GetStream());
			var clientReader = new MessageStreamReader(client.GetStream());

			// act - write on server
			const uint type1 = 16;
			var msg1 = new byte[] { 0, 1, 2, 5 };
			serverWriter.Enqueue(type1, msg1);
			serverWriter.Flush();

			// act - write on client
			const uint type2 = 7;
			var msg2 = new byte[] { 37, 14, 57, 75, 22 };
			clientWriter.Enqueue(type2, msg2);
			clientWriter.Flush();

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
				Assert.That(clientDmsg.hdr.Length, Is.EqualTo(4));
				CollectionAssert.AreEqual(msg1, clientDmsg.msg);
			});

			Assert.Multiple(() =>
			{
				Assert.That(serverDmsg.hdr.Type, Is.EqualTo(7));
				Assert.That(serverDmsg.hdr.Length, Is.EqualTo(5));
				CollectionAssert.AreEqual(msg2, serverDmsg.msg);
			});
		}
	}
}