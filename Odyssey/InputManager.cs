using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Odyssey.Network;

namespace Odyssey
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	public struct NetworkInput : INetworkMessage
	{
		public MouseState Mouse;
		public KeyboardState Keyboard;
		public GamePadState Gamepad;

		//public DateTime InputTime;
		public long InputTimeUnixMilliseconds;

		//public string PlayerName;

		public MessageType Type => MessageType.NetworkInput;

		public override bool Equals([NotNullWhen(true)] object obj)
		{
			if (Keyboard != ((NetworkInput)obj).Keyboard)
			{
				return false;
			}

			return true;
		}
	}

	public class InputManager : GameComponent
	{
		public InputManager(Game game) : base(game)
		{
		}

		public new void Update(GameTime gameTime) => base.Update(gameTime);//prevKeysDown = keysDown;//var mouse = Mouse.GetState();//var keyboard = Keyboard.GetState(PlayerIndex.One);//keysDown = keyboard.GetPressedKeys();//if ()
	}
}
