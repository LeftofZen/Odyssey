using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Odyssey.ECS;
using Odyssey.Logging;
using Serilog;

namespace Odyssey.Networking
{
	public class OdysseyServer
	{
		private List<OdysseyClient> clientList;
		private TcpListener clientNegotiator;
		private TcpClient client;
		private bool negotiatorRun = true;

		public IList<OdysseyClient> Clients => clientList;
		ILogger Logger;

		public OdysseyServer(ILogger logger)
		{
			clientList = new List<OdysseyClient>();
			Logger = logger;
		}
		public OdysseyServer()
		{
			clientList = new List<OdysseyClient>();
			Logger = Log.Logger;
		}

		private Task clientNegotiatorTask;

		public bool Start()
		{
			Logger.Information("[OdysseyServer::Start] Server starting on {name} {port}", Constants.DefaultHostname, Constants.DefaultPort);
			clientNegotiator = new TcpListener(Constants.DefaultHostname, Constants.DefaultPort);

			// Start listening for client requests.

			clientNegotiatorTask = new Task(ClientLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public int ClientCount => clientList.Count;

		public IEnumerable<(OdysseyClient, INetworkMessage)> GetReceivedMessages()
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

		public void Update(GameTime? gameTime)
		{
			clientList = clientList.Where(c => c.Connected).ToList();
			foreach (var c in clientList)
			{
				c.FlushMessages();
			}
		}

		public void Update()
			=> Update(null);

		public void SendMessageToAllClients<T>(T message) where T : struct, INetworkMessage
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

		public void SendMessageToAllClientsExcept<T>(T message, IEntity exceptedEntity) where T : struct, INetworkMessage
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

		public void SendMessageToClient<T>(Guid id, T message) where T : struct, INetworkMessage
		{
			Logger.Debug("[OdysseyServer::SendMessageToClient]");

			var client = clientList.SingleOrDefault(c => c.ControllingEntity.Id == id);
			if (client != null)
			{
				client.QueueMessage(message);
			}
		}

		private void ClientLoop()
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
				clientList.Add(oc);

				Thread.Sleep(100);
			}

			clientNegotiator.Stop();
			Logger.Debug("[OdysseyServer::ClientLoop] Client listener stopped!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
