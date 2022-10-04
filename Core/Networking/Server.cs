using System.Collections.Concurrent;
using System.Net.Sockets;
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
			clientNegotiator.Start();
			clientNegotiatorTask = new Task(ClientLoop);
			clientNegotiatorTask.Start();

			return true;
		}

		public void SendMessageToAllClients<T>(NetworkMessageType type, T message) where T : INetworkMessage
		{
			foreach (var c in clientList)
			{
				c.SendMessage(type, message);
			}
		}

		public void SendMessageToClient<T>(string playerName, NetworkMessageType type, T message) where T : INetworkMessage
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
			while (negotiatorRun)
			{
				Log.Debug("Waiting for a connection... ");

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
