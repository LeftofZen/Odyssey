using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog;

namespace Odyssey.Network
{
	public static class Helpers
	{
		public static T Deserialise<T>(ReadOnlySpan<byte> bytes)
		{
			var len = bytes.Length;
			var buffer = Marshal.AllocHGlobal(len);
			Marshal.Copy(bytes.ToArray(), 0, buffer, len); // copy to the alloc'd buffer"

			var tObj = (T)Marshal.PtrToStructure(buffer, typeof(T));
			Marshal.FreeHGlobal(buffer);
			return tObj;
		}

		public static ReadOnlySpan<byte> Serialise<T>(T tObj)
		{
			var len = Marshal.SizeOf(tObj);
			var arr = new byte[len];
			var ptr = Marshal.AllocHGlobal(len);

			Marshal.StructureToPtr(tObj, ptr, true);
			Marshal.Copy(ptr, arr, 0, len);
			Marshal.FreeHGlobal(ptr);

			return arr;
		}
	}

	public enum MessageType : int
	{ Header, Login, Logout, ChatMessage, NetworkInput }

	public interface INetworkMessage
	{
		MessageType Type { get; }
	}

	[StructLayout(LayoutKind.Sequential)]
	[Serializable]
	public struct MessageHeader : INetworkMessage
	{
		public MessageType Type { get; set; }
	}

	public static class Constants
	{
		public const int MessageHeaderSize = 8;
		public const int NetworkInputSize = 128;
	}

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

			tObj = Helpers.Deserialise<T>(bytes.AsSpan());
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
