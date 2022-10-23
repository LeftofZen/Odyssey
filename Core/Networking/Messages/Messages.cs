﻿using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Input;

namespace Odyssey.Networking.Messages
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct InputUpdate : INetworkMessage
	{
		public MouseState Mouse;
		public KeyboardState Keyboard;
		public GamePadState Gamepad;

		public long InputTimeUnixMilliseconds;

		//public string PlayerName;

		public NetworkMessageType Type => NetworkMessageType.NetworkInput;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct WorldUpdate : INetworkMessage
	{
		public NetworkMessageType Type => NetworkMessageType.WorldUpdate;

		//public Map Map { get; set; }
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct PlayerUpdate : INetworkMessage
	{
		public NetworkMessageType Type => NetworkMessageType.PlayerUpdate;

		//public Vector2 Position;
	}
}