using System.Collections.Concurrent;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using Odyssey.Networking;
using Serilog;

namespace Odyssey.Network
{
	public class OdysseyServer
	{
		private List<OdysseyClient> clientList;
		private TcpListener clientNegotiator;
		private TcpClient client;
		public ConcurrentQueue<INetworkMessage> MessageQueue;
		private bool negotiatorRun = true;

		public OdysseyServer()
		{
			clientList = new List<OdysseyClient>();
			MessageQueue = new ConcurrentQueue<INetworkMessage>();
		}

		private Task clientNegotiatorTask;

		public bool Start()
		{
			Log.Information("Server starting on {name} {port}", Networking.Constants.DefaultHostname, Networking.Constants.DefaultPort);
			clientNegotiator = new TcpListener(Networking.Constants.DefaultHostname, Networking.Constants.DefaultPort);

			// Start listening for client requests.

			clientNegotiatorTask = new Task(ClientLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public void Update(GameTime gameTime)
		{
			ReadMessages();

			// purge closed clients
			clientList = clientList.Where(c => c.Client.Connected).ToList();
		}

		public void SendMessageToAllClients<T>(NetworkMessageType type, T message) where T : struct, INetworkMessage
		{
			foreach (var c in clientList)
			{
				if (!c.SendMessage(type, message))
				{
					c.StopClient();
				}
			}
		}

		public void SendMessageToClient<T>(string playerName, NetworkMessageType type, T message) where T : struct, INetworkMessage
		{
			var client = clientList.SingleOrDefault(c => c.PlayerName == playerName);
			if (client != null)
			{
				client.SendMessage(type, message);
			}
		}

		public void ReadMessages()
		{
			foreach (var c in clientList)
			{
				c.ReadMessages();
			}
		}

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
					clientList.Add(new OdysseyClient(clientNegotiator.AcceptTcpClient()));
					Log.Debug("Connected!");
				}

				Thread.Sleep(100);
			}

			clientNegotiator.Stop();
			Log.Debug("Client listener stopped!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
