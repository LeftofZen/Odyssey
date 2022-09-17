using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Core.Networking;
using Serilog;

namespace Odyssey.Network
{

	public class Server
	{
		private List<TcpListener> clientList;
		private TcpListener clientNegotiator;
		public ConcurrentQueue<INetworkMessage> MessageQueue;
		private bool negotiatorRun = true;

		public IPAddress Hostname = IPAddress.Parse("127.0.0.1");
		public int Port = 13002;

		public Server()
		{
			clientList = new List<TcpListener>();
			MessageQueue = new ConcurrentQueue<INetworkMessage>();
		}

		private Task clientNegotiatorTask;

		public bool Start()
		{
			Log.Information("Server starting on {name} {port}", Hostname, Port);
			clientNegotiator = new TcpListener(Hostname, Port);

			// Start listening for client requests.
			//if (!clientNegotiator.)
			{
				clientNegotiator.Start();
				clientNegotiatorTask = new Task(ClientLoop);
				clientNegotiatorTask.Start();
			}

			return true;
		}

		public bool TryReadMessage<T>(NetworkStream stream, out T tObj) where T : INetworkMessage
		{
			// var a = Marshal.SizeOf(typeof(Microsoft.Xna.Framework.Input.MouseState));
			// var b = Marshal.SizeOf(typeof(Microsoft.Xna.Framework.Input.KeyboardState));
			// var c = Marshal.SizeOf(typeof(Microsoft.Xna.Framework.Input.GamePadState));
			// var d = Marshal.SizeOf(typeof(Int32));
			// Console.WriteLine($"{a},{b},{c},{d}");

			var bytes = new byte[Marshal.SizeOf(typeof(T))];
			var bytesRead = stream.Read(bytes, 0, bytes.Length);

			if (bytesRead != bytes.Length)
			{
				//var ex = new Exception("invalid message");
				Log.Warning("unknown network message");
				tObj = default;
				return false;
			}

			tObj = Protocol.Deserialise<T>(bytes.AsSpan());
			return true;
		}

		private void ClientLoop()
		{
			Log.Debug("Waiting for a connection... ");

			// Perform a blocking call to accept requests.
			// You could also use server.AcceptSocket() here.
			var client = clientNegotiator.AcceptTcpClient();
			Log.Debug("Connected!");
			var clientStream = client.GetStream();

			while (negotiatorRun)
			{
				if (TryReadMessage<MessageHeader>(clientStream, out var header))
				{
					Log.Debug("[Header Received] Type={msg}", header.Type);

					switch (header.Type)
					{
						case MessageType.NetworkInput:
							if (TryReadMessage<NetworkInput>(clientStream, out var input))
							{
								MessageQueue.Enqueue(input);
							}
							break;
					}
				}

				//int i;
				//// Loop to receive all the data sent by the client.
				//while ((i = stream.Read(msgHeaderBuffer, 0, msgHeaderBuffer.Length)) != 0)
				//{
				//	// Translate data bytes to a ASCII string.
				//	data = System.Text.Encoding.ASCII.GetString(msgHeaderBuffer, 0, i);
				//	Console.WriteLine("Received: {0}", data);

				//	// Process the data sent by the client.
				//	data = data.ToUpper();

				//	var msg = System.Text.Encoding.ASCII.GetBytes(data);

				//	// Send back a response.
				//	stream.Write(msg, 0, msg.Length);
				//	Console.WriteLine("Sent: {0}", data);
				//}

				//// Shutdown and end connection
				//client.Close();
				//const int inputTickRate = 144; // 200hz
				//Thread.Sleep((int)(1000f / inputTickRate));
			}

			clientNegotiator.Stop();
			Log.Debug("Disconnected!");
		}

		public bool Stop() => negotiatorRun = false;
	}
}
