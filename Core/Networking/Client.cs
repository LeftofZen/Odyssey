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

		private MessageStreamWriter writer;
		private MessageStreamReader reader;

		// TODO: fix
		public Queue<INetworkMessage>? Messages => new(); // reader?.MessageQueue;

		public OdysseyClient(IPAddress hostname, int port)
		{
			Address = hostname;
			Port = port;
			TcpClient = new TcpClient();

			Log.Debug("New OdysseyClient via endpoint {hostname} {port}", Address, Port);
		}

		public OdysseyClient(TcpClient client)
		{
			Address = (client.Client.RemoteEndPoint as IPEndPoint).Address;
			Port = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
			TcpClient = client;

			Log.Debug("New OdysseyClient via endpoint {hostname} {port}", Address, Port);

			InitMessaging();
		}

		private void InitMessaging()
		{
			Log.Debug("[InitMessaging] {connected}", TcpClient.Connected);
			if (TcpClient.Connected)
			{
				readMsgs = true;

				writer = new MessageStreamWriter(TcpClient.GetStream());
				reader = new MessageStreamReader(TcpClient.GetStream(), MessageLookup.ToTypeFunc);

				msgReaderTask = Task.Run(ReadMessages);
			}
		}

		public void Start()
		{
			Log.Information("[Start] Client connecting on {name} {port}", Address, Port);
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
			Log.Information("[Login] {user} {pass}", "Bob", "Foo");
			LoginMessageInFlight = true;
			return QueueMessage(new LoginRequest() { Username = "Bob", Password = "Foo" });
		}

		public void StopClient() => TcpClient.Close();

		private bool readMsgs;

		public void ReadMessages()
		{
			Log.Debug("[ReadMessages] Client message loop starting");
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
						Log.Error("[ReadMessages] Message reader is null");
						return;
					}
				}

				reader.Update();
			}
		}

		public void FlushMessages() => writer.Flush();

		public bool QueueMessage<T>(T message) where T : struct, INetworkMessage
		{
			Log.Debug("[QueueMessage] {0}", message);

			if (writer is null)
			{
				InitMessaging();

				if (writer is null)
				{
					Log.Error("[QueueMessage] Message writer is null");
					return false;
				}
			}

			writer.Enqueue(message);
			return true;
		}
	}
}
