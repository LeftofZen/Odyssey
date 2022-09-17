using System.Net;
using System.Net.Sockets;
using Core.Networking;
using Serilog;

namespace Odyssey.Network
{
	public class Client
	{
		public IPAddress Hostname;
		public int Port;
		private TcpClient tcpClient;

		public Client(IPAddress hostname, int port)
		{
			Hostname = hostname;
			// debug
			Hostname = IPAddress.Parse("127.0.0.1");
			Port = port;
			tcpClient = new TcpClient();
		}

		public void Start()
		{
			Log.Information("Client connecting on {name} {port}", Hostname, Port);
			//tcpClient = new TcpClient();
			tcpClient.Connect(new IPEndPoint(Hostname, Port));
		}

		public void StopClient() => tcpClient.Close();

		public void Update()
		{

		}

		public void SendMessage<T>(MessageType type, T message) where T : INetworkMessage
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
			stream.Write(Protocol.Serialise(messageHeader));
			stream.Write(Protocol.Serialise(message));

			stream.Flush();
			//Log.Debug("SendMessage");
			//stream.Close();
		}
	}
}
