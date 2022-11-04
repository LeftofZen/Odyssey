using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Serilog;

namespace Odyssey.Networking
{
	public class OdysseyServer
	{
		private List<OdysseyClient> clientList;
		private TcpListener clientNegotiator;
		private TcpClient client;
		private bool negotiatorRun = true;

		public OdysseyServer() => clientList = new List<OdysseyClient>();

		private Task clientNegotiatorTask;

		public bool Start()
		{
			Log.Information("Server starting on {name} {port}", Constants.DefaultHostname, Constants.DefaultPort);
			clientNegotiator = new TcpListener(Constants.DefaultHostname, Constants.DefaultPort);

			// Start listening for client requests.

			clientNegotiatorTask = new Task(ClientLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public IEnumerable<(OdysseyClient, INetworkMessage)> GetServerMessages()
		{
			foreach (var c in clientList)
			{
				if (c?.Messages is not null)
				{
					while (c.Messages.TryDequeue(out var m))
					{
						yield return (c, m);
					}
				}
			}
		}

		public IEnumerable<IEntity> GetClients()
		{
			foreach (var c in clientList)
			{
				yield return c.ControllingEntity;
			}
		}

		public IEnumerable<IEntity> GetConnectedEntities()
		{
			foreach (var c in clientList)
			{
				yield return c.ControllingEntity;
			}
		}

		public void Update(GameTime gameTime) =>
			// copies messages to their respective client queues
			//ReadMessages();

			// purge closed clients
			clientList = clientList.Where(c => c.TcpClient.Connected).ToList();

		public void SendMessageToAllClients<T>(T message) where T : struct, INetworkMessage
		{
			foreach (var c in clientList)
			{
				if (!c.QueueMessage(message))
				{
					c.StopClient();
				}
			}
		}

		public void SendMessageToAllClientsExcept<T>(T message, IEntity exceptedEntity) where T : struct, INetworkMessage
		{
			foreach (var c in clientList)
			{
				if (c.ControllingEntity != null && c.ControllingEntity.Id != exceptedEntity.Id)
				{
					if (!c.QueueMessage(message))
					{
						c.StopClient();
					}
				}
			}
		}

		public void SendMessageToClient<T>(Guid id, T message) where T : struct, INetworkMessage
		{
			var client = clientList.SingleOrDefault(c => c.ControllingEntity.Id == id);
			if (client != null)
			{
				client.QueueMessage(message);
			}
		}

		//public void ReadMessages()
		//{
		//	foreach (var c in clientList)
		//	{
		//		c.ReadMessages();
		//	}
		//}

		private void ClientLoop()
		{
			Log.Debug("Waiting for a connection... ");

			clientNegotiator.Start();
			while (negotiatorRun)
			{
				// Perform a blocking call to accept requests.
				// You could also use server.AcceptSocket() here.
				if (clientNegotiator.Pending())
				{
					var client = clientNegotiator.AcceptTcpClient();
					clientList.Add(new OdysseyClient(client));
					Log.Debug("[ClientLoop] Connected! {connected} {endpoint}", client.Client.Connected, client.Client.RemoteEndPoint.ToString());
				}

				Thread.Sleep(100);
			}

			clientNegotiator.Stop();
			Log.Debug("Client listener stopped!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
