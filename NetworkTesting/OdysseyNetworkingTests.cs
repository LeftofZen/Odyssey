using Odyssey.Networking;
using Odyssey.Networking.Messages;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Testing
{
	public class OdysseyNetworkingTests
	{
		private OdysseyServer server;
		private OdysseyClient client;

		[SetUp]
		public void SetUp()
		{
			server = new OdysseyServer();
			Assert.That(server.Start(), Is.True);
			Assert.That(server.ClientCount, Is.Zero);

			client = new OdysseyClient(Constants.DefaultHostname, Constants.DefaultPort);
			Assert.That(client.Connect(), Is.True);

			Assert.That(client.Connected, Is.True);
			Assert.That(server.ClientCount, Is.EqualTo(1));
		}

		[TearDown]
		public void TearDown()
		{
			client.Disconnect();
			//Assert.That(client.Connected, Is.False);
			//Assert.That(server.ClientCount, Is.Zero);
		}

		[Test]
		public void TestServerClientSendMessage()
		{
			var msg = new BroadcastMessage() { Message = "Hello World" };
			server.SendMessageToAllClients(msg);
			server.Update();

			var sw = new Stopwatch();
			sw.Restart();

			while (sw.Elapsed.TotalSeconds < 5)
			{
				if (client.TryDequeueMessage(out var msgc) && msgc.msg is BroadcastMessage bmsg)
				{
					Assert.That(bmsg.Message, Is.EqualTo("Hello World"));
					return;
				}

				Thread.Sleep(100);
			}

			Assert.Fail("Didn't get any messages");
		}

		[Test]
		public void TestClientServerSendMessage()
		{
			Assert.Fail();
		}
	}
}
