using Odyssey.Messaging;
using Odyssey.Networking;
using System.Diagnostics;

namespace Network
{
	[TestFixture]
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

			client = new OdysseyClient(Odyssey.Networking.Constants.DefaultHostname, Odyssey.Networking.Constants.DefaultPort);
			Assert.That(client.Connect(), Is.True);

			Assert.That(client.Connected, Is.True);
			Assert.That(server.ClientCount, Is.EqualTo(1));
		}

		[TearDown]
		public void TearDown()
		{
			client.Disconnect();
			client.Dispose();
			//Assert.That(client.Connected, Is.False);
			//Assert.That(server.ClientCount, Is.Zero);
		}

		[Test]
		public void ServerClientSendMessage()
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
		public void ClientServerSendMessage()
		{
			Assert.Fail();
		}
	}
}
