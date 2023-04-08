using System.Runtime.InteropServices;
using MessagePack;
using Microsoft.Xna.Framework.Input;

// TODO - autogenerate this file with source generators https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/
namespace Odyssey.Networking.Messages
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct InputUpdate : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.InputUpdate;

		public MouseState Mouse;
		public KeyboardState Keyboard;
		public GamePadState Gamepad;

		public long InputTimeUnixMilliseconds;

		public Guid ClientId { get; init; }

		public bool RequiresLogin => true;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct WorldUpdate : INetworkMessage
	{
		public uint Type => (uint)NetworkMessageType.WorldUpdate;
		public bool RequiresLogin => false;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct PlayerUpdate : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.PlayerUpdate;
		public Guid ClientId { get; init; }

		//public Vector2 Position;

		public bool RequiresLogin => false;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct LoginRequest : INetworkMessage
	{
		public uint Type => (uint)NetworkMessageType.LoginRequest;
		public string Username { get; init; }
		public string Password { get; init; }
		public bool RequiresLogin => false;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct LoginResponse : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.LoginResponse;
		public Guid ClientId { get; init; }
		public bool RequiresLogin => false;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct LogoutRequest : INetworkMessage
	{
		public uint Type => (uint)NetworkMessageType.LogoutRequest;
		public bool RequiresLogin => true;
		public string Username { get; init; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct LogoutResponse : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.LogoutResponse;
		public Guid ClientId { get; init; }
		public bool RequiresLogin => true;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct ChatMessage : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.ChatMessage;
		public Guid ClientId { get; init; }
		public bool RequiresLogin => true;
		public string Message { get; init; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct BroadcastMessage : INetworkMessage
	{
		public uint Type => (uint)NetworkMessageType.Broadcast;
		public bool RequiresLogin => false;
		public string Message { get; init; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct KeepAliveMessage : INetworkMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.KeepAlive;
		public Guid ClientId { get; init; }
		public bool RequiresLogin => false;
		public long Timestamp { get; init; }
	}

}
