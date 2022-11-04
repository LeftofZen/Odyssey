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
			Log.Information("[OdysseyServer::Start] Server starting on {name} {port}", Constants.DefaultHostname, Constants.DefaultPort);
			clientNegotiator = new TcpListener(Constants.DefaultHostname, Constants.DefaultPort);

			// Start listening for client requests.

			clientNegotiatorTask = new Task(ClientLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public IEnumerable<(OdysseyClient, INetworkMessage)> GetReceivedMessages()
		{
			foreach (var client in clientList)
			{
				if (client?.Messages is not null)
				{
					while (client.Messages.TryDequeue(out var dmsg))
					{
						Log.Information("[OdysseyServer::GetServerMessages] {msgType}", dmsg.hdr.Type);
						yield return (client, dmsg.msg);
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

		public void Update(GameTime gameTime)
		{
			clientList = clientList.Where(c => c.TcpClient.Connected).ToList();
			foreach (var c in clientList)
			{
				c.FlushMessages();
			}
		}


		public void SendMessageToAllClients<T>(T message) where T : struct, INetworkMessage
		{
			Log.Debug("[OdysseyServer::SendMessageToAllClients]");

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
			Log.Debug("[OdysseyServer::SendMessageToAllClientsExcept]");

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
			Log.Debug("[OdysseyServer::SendMessageToClient]");

			var client = clientList.SingleOrDefault(c => c.ControllingEntity.Id == id);
			if (client != null)
			{
				client.QueueMessage(message);
			}
		}

		private void ClientLoop()
		{
			Log.Debug("[OdysseyServer::ClientLoop] {negotiatorRun}", negotiatorRun);

			clientNegotiator.Start();
			while (negotiatorRun)
			{
				Log.Debug("[OdysseyServer::ClientLoop] Waiting for a connection... ");

				// Perform a blocking call to accept requests.
				var client = clientNegotiator.AcceptTcpClient();

				Log.Debug("[OdysseyServer::ClientLoop] Connected! {connected} {endpoint}", client.Client.Connected, client.Client.RemoteEndPoint.ToString());
				clientList.Add(new OdysseyClient(client));

				Thread.Sleep(10);
			}

			clientNegotiator.Stop();
			Log.Debug("[OdysseyServer::ClientLoop] Client listener stopped!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
