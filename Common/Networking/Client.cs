using Messaging.MessagePack;
using Messaging.Reading;
using Messaging.Writing;
using Odyssey.ECS;
using Odyssey.Messaging.Messages;
using Serilog;
using System.Net;
using System.Net.Sockets;

namespace Odyssey.Messaging
{
	public class OdysseyClient : IDisposable
	{
		private TcpClient tcpClient;

		public IPEndPoint Endpoint { get; init; }

		public IEntity? ControllingEntity;
		private MessageStreamWriter<IMessage>? writer;
		private MessageStreamReader<IMessage>? reader;

		public bool IsLoggedIn { get; set; }
		public bool LoginMessageInFlight { get; set; }
		public bool LogoutMessageInFlight { get; set; }

		// manual connection verification 
		private bool _connected = false;
		public bool Connected
			=> tcpClient != null && tcpClient.Connected; // && _connected;

		//public Queue<(Header hdr, INetworkMessage msg)>? Messages => reader?.DelimitedMessageQueue;
		public bool TryDequeueMessage(out (Header hdr, IMessage msg) msg)
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

		public void InitMessaging()
		{
			Log.Debug("[Client::InitMessaging] {connected}", tcpClient.Connected);
			if (tcpClient.Connected)
			{
				readMsgs = true;

				writer = new MessageStreamWriter<IMessage>(tcpClient.GetStream(), new MessagePackSerialiser());
				reader = new MessageStreamReader<IMessage>(tcpClient.GetStream(), new MessagePackDeserialiser<NetworkMessageType>(new MessageLookup()));

				msgReaderTask = Task.Run(ReadMessageLoop);
			}
			else
			{
				throw new InvalidOperationException("cannot init messaging if client isn't connected");
			}
		}

		public async Task<bool> ConnectAsync()
		{
			Log.Information("[Client::Connect] Client connecting on {endpoint}", Endpoint);

			try
			{
				bool isConnected = false;
				try
				{
					isConnected = tcpClient.Connected;
				}
				catch (ObjectDisposedException)
				{
					isConnected = false;
				}

				if (!isConnected)
				{
					// TcpClient cannot be reused after Dispose/Close. We must create a new instance.
					tcpClient = new TcpClient();

					await tcpClient.ConnectAsync(Endpoint);
					if (!tcpClient.Connected)
					{
						throw new InvalidOperationException("client couldn't connect");
					}

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

		public bool Connect()
			=> ConnectAsync().Result;

		private Task msgReaderTask;

		public bool Login(string user, string pass)
		{
			if (!LoginMessageInFlight)
			{
				Log.Information("[Client::Login] {user} {pass}", user, pass);
				LoginMessageInFlight = true;
				return QueueMessage(new LoginRequest() { Username = user, Password = pass });
			}

			Log.Information("[Client::Login] LoginMessage is already in-flight");
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

			Log.Information("[Client::Logout] LogoutMessage is already in-flight");
			return false;
		}

		public void Disconnect()
		{
			Log.Information("[Client::Disconnect]");
			readMsgs = false;
			tcpClient.Close(); // cancel/join msgReaderTask as well
			tcpClient.Dispose();
		}

		private bool readMsgs;

		private void ReadMessageLoop()
		{
			Log.Debug("[Client::ReadMessageLoop] Client message loop starting {readMsgs}", readMsgs);
			while (readMsgs)
			{
				if (!tcpClient.Connected)
				{
					Log.Information("[Client::ReadMessageLoop] Client disconnected from server. Aborting message loop");
					break;
				}

				//if (reader is null)
				//{
				//	InitMessaging();

				//	if (reader is null)
				//	{
				//		Log.Error("[Client::ReadMessageLoop] Message reader is null");
				//		break;
				//	}
				//}

				if (reader is not null)
				{
					reader.Update();
				}
			}

			Log.Information("[Client::ReadMessageLoop] Loop terminated");
		}

		public int PendingMessages => writer?.PendingMessages ?? 0;

		// this will ALWAYS send AT LEAST ONE message. it'll either be whatever the consumer wants to send, or a keep-alive message
		public void FlushMessages()
		{
			try
			{
				if (writer is null)
				{
					return;
				}

				if (writer.PendingMessages == 0)
				{
					//writer.Enqueue(new KeepAliveMessage() { ClientId = ControllingEntity?.Id ?? Guid.Empty, Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds() });
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

		public bool QueueMessage<T>(T message) where T : struct, IMessage
		{
			Log.Debug("[Client::QueueMessage] {type} {ctype}", message.Type, message.GetType());

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

		public void Dispose()
		{
			tcpClient.Dispose();
		}
	}
}
