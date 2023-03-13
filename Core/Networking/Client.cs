using System.Net;
using System.Net.Sockets;
using Odyssey.Networking.Messages;
using Serilog;

namespace Odyssey.Networking
{
	public class OdysseyClient
	{
		private TcpClient tcpClient;

		public IPEndPoint Endpoint { get; init; }

		public IEntity? ControllingEntity;
		private MessageStreamWriter<INetworkMessage>? writer;
		private MessageStreamReader<INetworkMessage>? reader;

		public bool IsLoggedIn { get; set; }
		public bool LoginMessageInFlight { get; set; }
		public bool LogoutMessageInFlight { get; set; }

		// manual connection verification 
		private bool _connected = false;
		public bool Connected => tcpClient != null && tcpClient.Connected && _connected;

		//public Queue<(Header hdr, INetworkMessage msg)>? Messages => reader?.DelimitedMessageQueue;
		public bool TryDequeueMessage(out (Header hdr, INetworkMessage msg) msg)
		{
			if (reader != null && reader.TryDequeue(out msg))
			{
				return true;
			}

			msg = default;
			return false;
		}

		public string ConnectionDetails => $"[{Endpoint.Address}:{Endpoint.Port}] Connected={Connected} LoggedIn={IsLoggedIn}";

		public OdysseyClient(IPAddress address, int port)
		{
			Endpoint = new IPEndPoint(address, port);
			tcpClient = new TcpClient();
			Log.Debug("[Client::OdysseyClient] New OdysseyClient via endpoint {hostname} {port}", Endpoint.Address, Endpoint.Port);
		}

		public OdysseyClient(TcpClient client)
		{
			Endpoint = client.Client.RemoteEndPoint as IPEndPoint;
			tcpClient = client;
			Log.Debug("[Client::OdysseyClient] New OdysseyClient via endpoint {hostname} {port}", Endpoint.Address, Endpoint.Port);
		}

		private void InitMessaging()
		{
			Log.Debug("[Client::InitMessaging] {connected}", tcpClient.Connected);
			if (tcpClient.Connected)
			{
				readMsgs = true;

				writer = new MessageStreamWriter<INetworkMessage>(tcpClient.GetStream(), new MessagePackSerialiser());
				reader = new MessageStreamReader<INetworkMessage>(tcpClient.GetStream(), new MessagePackDeserialiser());

				msgReaderTask = Task.Run(ReadMessageLoop);
			}
		}

		public bool Connect()
		{
			Log.Information("[Client::Start] Client connecting on {endpoint}", Endpoint);
			//tcpClient = new TcpClient();

			try
			{
				if (!tcpClient.Connected)
				{
					tcpClient.Connect(Endpoint);
					InitMessaging();
				}
				return true;
			}
			catch (Exception ex)
			{
				Log.Error("Ex={0}", ex);
				return false;
			}
		}

		private Task msgReaderTask;

		public bool Login(string user, string pass)
		{
			if (!LoginMessageInFlight)
			{
				Log.Information("[Client::Login] {user} {pass}", user, pass);
				LoginMessageInFlight = true;
				return QueueMessage(new LoginRequest() { Username = user, Password = pass });
			}
			return false;
		}

		public bool Logout(string user)
		{
			if (!LogoutMessageInFlight)
			{
				Log.Information("[Client::Logout] {user}", user);
				LogoutMessageInFlight = true;
				return QueueMessage(new LogoutRequest() { Username = user });
			}
			return false;
		}

		public void Disconnect()
		{
			tcpClient.Close(); // cancel/join msgReaderTask as well
			tcpClient.Dispose();
		}

		private bool readMsgs;

		public void ReadMessageLoop()
		{
			Log.Debug("[Client::ReadMessages] Client message loop starting {readMsgs}", readMsgs);
			while (readMsgs)
			{
				if (!tcpClient.Connected)
				{
					Log.Information("[Client::ReadMessages] Client disconnected from server. Aborting message loop");
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

		// this will ALWAYS send AT LEAST ONE message. it'll either be whatever the consumer wants to send, or a keep-alive message
		public void FlushMessages()
		{
			try
			{
				if (writer.PendingMessages == 0)
				{
					writer.Enqueue(new KeepAliveMessage() { ClientId = ControllingEntity?.Id ?? Guid.Empty, Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });
				}

				writer.Update();

				// if we were able to send a message, we are now connected as far as tcp is concerned
				_connected = true;
			}
			catch (Exception)
			{
				tcpClient.Close();
				_connected = false;
			}
		}

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
