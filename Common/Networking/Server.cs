using Microsoft.Xna.Framework;
using Odyssey.ECS;
using Serilog;
using System.Net.Sockets;

namespace Odyssey.Messaging
{
	public class OdysseyServer
	{
		private List<OdysseyClient> clientList;
		private TcpListener clientNegotiator;
		private TcpClient client;
		private bool negotiatorRun = true;

		public IList<OdysseyClient> Clients => clientList;

		private ILogger Logger;

		public OdysseyServer(ILogger logger)
		{
			clientList = [];
			Logger = logger;
		}
		public OdysseyServer()
		{
			clientList = [];
			Logger = Log.Logger;
		}

		private Task clientNegotiatorTask;

		public bool Start()
		{
			Logger.Information("[OdysseyServer::Start] Server starting on {name} {port}", Networking.Constants.DefaultHostname, Networking.Constants.DefaultPort);
			clientNegotiator = new TcpListener(Networking.Constants.DefaultHostname, Networking.Constants.DefaultPort);

			// Start listening for client requests.

			clientNegotiatorTask = new Task(ClientNegotiatorLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public int ClientCount => clientList.Count;

		public IEnumerable<(OdysseyClient, IMessage)> GetReceivedMessages()
		{
			foreach (var client in clientList)
			{
				while (client.TryDequeueMessage(out var dmsg))
				{
					Logger.Information("[OdysseyServer::GetServerMessages] {msgType}", dmsg.hdr.Type);
					yield return (client, dmsg.msg);
				}
			}
		}

		public IEnumerable<IEntity> GetConnectedEntities()
		{
			foreach (var c in clientList)
			{
				yield return c.ControllingEntity;
			}
		}

		public void DisconnectClient(OdysseyClient client)
		{
			Logger.Information("[OdysseyServer::DisconnectClient] Disconnecting [{client}]", client.ConnectionDetails);
			client.Disconnect();
			clientList.Remove(client);
		}

		public void Update(GameTime? gameTime)
		{
			foreach (var c in clientList.Where(c => c.Connected))
			{
				c.FlushMessages();
			}
		}

		public void Update()
			=> Update(null);

		public void SendMessageToAllClients<T>(T message) where T : struct, IMessage
		{
			Logger.Debug("[OdysseyServer::SendMessageToAllClients]");

			foreach (var c in clientList)
			{
				if (!c.QueueMessage(message))
				{
					c.Disconnect();
				}
			}
		}

		public void SendMessageToAllClientsExcept<T>(T message, IEntity exceptedEntity) where T : struct, IMessage
		{
			Logger.Debug("[OdysseyServer::SendMessageToAllClientsExcept]");

			foreach (var c in clientList)
			{
				if (c.ControllingEntity != null && c.ControllingEntity.Id != exceptedEntity.Id)
				{
					if (!c.QueueMessage(message))
					{
						c.Disconnect();
					}
				}
			}
		}

		public void SendMessageToClient<T>(Guid id, T message) where T : struct, IMessage
		{
			Logger.Debug("[OdysseyServer::SendMessageToClient]");

			var client = clientList.SingleOrDefault(c => c.ControllingEntity.Id == id);
			_ = client?.QueueMessage(message);
		}

		private void ClientNegotiatorLoop()
		{
			Logger.Debug("[OdysseyServer::ClientLoop] {negotiatorRun}", negotiatorRun);

			clientNegotiator.Start();
			while (negotiatorRun)
			{
				Logger.Debug("[OdysseyServer::ClientLoop] Waiting for a connection... ");

				// Perform a blocking call to accept requests.
				var client = clientNegotiator.AcceptTcpClient();

				Logger.Debug("[OdysseyServer::ClientLoop] Connected! {connected} {endpoint}", client.Client.Connected, client.Client.RemoteEndPoint.ToString());
				var oc = new OdysseyClient(client);
				oc.InitMessaging();
				clientList.Add(oc);

				Thread.Sleep(100);
			}

			clientNegotiator.Stop();
			Logger.Debug("[OdysseyServer::ClientLoop] Client listener stopped!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
