using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace Core.Networking
{
	public static class Protocol
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

	public enum MessageType
	{
		Header,
		Login,
		Logout,
		ChatMessage,
		NetworkInput
	}

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

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct NetworkInput : INetworkMessage
	{
		public MouseState Mouse;
		public KeyboardState Keyboard;
		public GamePadState Gamepad;

		public long InputTimeUnixMilliseconds;

		//public string PlayerName;

		public MessageType Type => MessageType.NetworkInput;
	}
}
