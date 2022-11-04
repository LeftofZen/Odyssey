using System.Net;
using System.Net.Sockets;
using Odyssey.Networking.Messages;
using Serilog;

namespace Odyssey.Networking
{
	public class OdysseyClient
	{
		public IPAddress Address;
		public int Port;
		public TcpClient TcpClient { get; }
		public IEntity ControllingEntity;
		public bool IsLoggedIn { get; set; }
		public bool LoginMessageInFlight { get; set; }

		private MessageStreamWriter<INetworkMessage> writer;
		private MessageStreamReader<INetworkMessage> reader;

		public Queue<(Header hdr, INetworkMessage msg)>? Messages => reader?.DelimitedMessageQueue;

		public OdysseyClient(IPAddress hostname, int port)
		{
			Address = hostname;
			Port = port;
			TcpClient = new TcpClient();

			Log.Debug("[Client::OdysseyClient] New OdysseyClient via endpoint {hostname} {port}", Address, Port);
		}

		public OdysseyClient(TcpClient client)
		{
			Address = (client.Client.RemoteEndPoint as IPEndPoint).Address;
			Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
			TcpClient = client;

			Log.Debug("[Client::OdysseyClient] New OdysseyClient via endpoint {hostname} {port}", Address, Port);

			InitMessaging();
		}

		private void InitMessaging()
		{
			Log.Debug("[Client::InitMessaging] {connected}", TcpClient.Connected);
			if (TcpClient.Connected)
			{
				readMsgs = true;

				writer = new MessageStreamWriter<INetworkMessage>(TcpClient.GetStream(), new MessagePackSerialiser());
				reader = new MessageStreamReader<INetworkMessage>(TcpClient.GetStream(), new MessagePackDeserialiser());

				msgReaderTask = Task.Run(ReadMessageLoop);
			}
		}

		public void Start()
		{
			Log.Information("[Client::Start] Client connecting on {name} {port}", Address, Port);
			//tcpClient = new TcpClient();

			try
			{
				TcpClient.Connect(new IPEndPoint(Address, Port));
				InitMessaging();
			}
			catch (Exception ex)
			{
				Log.Error("Ex={0}", ex);
			}
		}

		private Task msgReaderTask;

		public bool Login(string user, string pass)
		{
			if (LoginMessageInFlight == false)
			{
				Log.Information("[Client::Login] {user} {pass}", "Bob", "Foo");
				LoginMessageInFlight = true;
				return QueueMessage(new LoginRequest() { Username = "Bob", Password = "Foo" });
			}
			return false;
		}

		public void StopClient() => TcpClient.Close(); // cancel/join msgReaderTask as well

		private bool readMsgs;

		public void ReadMessageLoop()
		{
			Log.Debug("[Client::ReadMessages] Client message loop starting {readMsgs}", readMsgs);
			while (readMsgs)
			{
				if (!TcpClient.Connected)
				{
					return;
				}

				if (reader is null)
				{
					InitMessaging();

					if (reader is null)
					{
						Log.Error("[Client::ReadMessages] Message reader is null");
						return;
					}
				}

				reader.Update();
			}
		}

		public void FlushMessages() => writer.Update();

		public bool QueueMessage<T>(T message) where T : struct, INetworkMessage
		{
			Log.Debug("[Client::QueueMessage] {type}", message.Type);

			if (writer is null)
			{
				InitMessaging();

				if (writer is null)
				{
					Log.Error("[Client::QueueMessage] Message writer is null");
					return false;
				}
			}

			writer.Enqueue(message);
			return true;
		}
	}
}
