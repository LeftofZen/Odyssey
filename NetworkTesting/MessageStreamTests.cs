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
		public void TestSimplex_Single()
		{
			// arrange
			var writer = new MessageStreamWriter<byte[]>(server.GetStream(), new ByteSerialiser());
			var reader = new MessageStreamReader<byte[]>(client.GetStream(), new ByteDeserialiser());

			// act - write a message from server to client
			const uint type = 16;
			var msg = new byte[] { 0, 1, 2, 5 };
			writer.EnqueueRaw(type, msg);
			writer.Update();

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
		public void TestSimplex_Multiple()
		{
			// arrange
			var writer = new MessageStreamWriter<byte[]>(server.GetStream(), new ByteSerialiser());
			var reader = new MessageStreamReader<byte[]>(client.GetStream(), new ByteDeserialiser());

			// act - write a message from server to client
			const uint type1 = 16;
			var msg1 = new byte[] { 0, 1, 2, 5 };
			writer.EnqueueRaw(type1, msg1);

			const uint type2 = 1;
			var msg2 = new byte[] { 19, 23, 45, 57, 53, 78, 97 };
			writer.EnqueueRaw(type2, msg2);

			const uint type3 = 158;
			var msg3 = new byte[] { 9, 8, 6, 4, 3 };
			writer.EnqueueRaw(type3, msg3);

			writer.Update();
			Thread.Sleep(100);
			reader.Update(); // don't need this update, but its good to test updating in the middle of the stream

			const uint type4 = 63;
			var msg4 = new byte[] { 1, 1, 1, 2, 1, 1, 1, 99 };
			writer.EnqueueRaw(type4, msg4);

			writer.Update();
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
		public void TestDuplex()
		{
			// arrange
			var serverWriter = new MessageStreamWriter<byte[]>(server.GetStream(), new ByteSerialiser());
			var serverReader = new MessageStreamReader<byte[]>(server.GetStream(), new ByteDeserialiser());
			var clientWriter = new MessageStreamWriter<byte[]>(client.GetStream(), new ByteSerialiser());
			var clientReader = new MessageStreamReader<byte[]>(client.GetStream(), new ByteDeserialiser());

			// act - write on server
			const uint type1 = 16;
			var msg1 = new byte[] { 0, 1, 2, 5 };
			serverWriter.EnqueueRaw(type1, msg1);
			serverWriter.Update();

			// act - write on client
			const uint type2 = 7;
			var msg2 = new byte[] { 37, 14, 57, 75, 22 };
			clientWriter.EnqueueRaw(type2, msg2);
			clientWriter.Update();

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