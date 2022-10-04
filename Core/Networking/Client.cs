using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Odyssey.Networking;
using Odyssey.Networking.Messages;
using Serilog;

namespace Odyssey.Network
{
	public class OdysseyClient
	{
		public IPAddress Address;
		public int Port;
		private TcpClient tcpClient;
		public ConcurrentQueue<INetworkMessage> MessageQueue;
		public string PlayerName;

		public TcpClient Client => tcpClient;

		public OdysseyClient(IPAddress hostname, int port)
		{
			Address = hostname;
			Port = port;
			tcpClient = new TcpClient();
			MessageQueue = new ConcurrentQueue<INetworkMessage>();
		}
		public OdysseyClient(TcpClient client)
		{
			Address = ((IPEndPoint)client.Client.RemoteEndPoint).Address;
			Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
			tcpClient = client;
			MessageQueue = new ConcurrentQueue<INetworkMessage>();
		}

		public void Start()
		{
			Log.Information("Client connecting on {name} {port}", Address, Port);
			//tcpClient = new TcpClient();
			tcpClient.Connect(new IPEndPoint(Address, Port));
		}

		public void StopClient() => tcpClient.Close();

		public void ReadMessages()
		{
			if (!tcpClient.Connected)
			{
				return;
			}

			var stream = tcpClient.GetStream();
			//while (true)
			{
				if (stream.TryReadMessage<MessageHeader>(out var header))
				{
					Log.Debug("[Header Received] Type={msg}", header.Type);

					switch (header.Type)
					{
						case NetworkMessageType.NetworkInput:
							if (stream.TryReadMessage<NetworkInput>(out var ni))
							{
								MessageQueue.Enqueue(ni);
							}
							break;
						case NetworkMessageType.WorldUpdate:
							if (stream.TryReadMessage<WorldUpdate>(out var wu))
							{
								MessageQueue.Enqueue(wu);
							}
							break;
						case NetworkMessageType.PlayerUpdate:
							if (stream.TryReadMessage<PlayerUpdate>(out var pu))
							{
								MessageQueue.Enqueue(pu);
							}
							break;
					}
				}
				//Thread.Sleep(100);
			}
		}

		public void SendMessage<T>(NetworkMessageType type, T message) where T : INetworkMessage
		{
			if (tcpClient == null)
			{
				return;
			}

			if (!tcpClient.Connected)
			{
				Log.Error("couldn't connect to server");
				return;
			}

			var stream = tcpClient.GetStream();

			var messageHeader = new MessageHeader() { Type = type };

			if (stream.Socket.Connected)
			{
				stream.Write(Protocol.Serialise(messageHeader));
				stream.Write(Protocol.Serialise(message));

				stream.Flush();
			}

			//Log.Debug("SendMessage");
			//stream.Close();
		}
	}
}
