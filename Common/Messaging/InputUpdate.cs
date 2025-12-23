using System;
using System.Runtime.InteropServices;
using MessagePack;
using Microsoft.Xna.Framework.Input;

namespace Odyssey.Messaging
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct InputUpdate : IMessage, IClientId
	{
		public uint Type => (uint)NetworkMessageType.InputUpdate;

		public MouseInputData Mouse;
		public KeyboardInputData Keyboard;
		public GamePadInputData Gamepad;

		public long InputTimeUnixMilliseconds;

		public Guid ClientId { get; init; }

		public bool RequiresLogin => true;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct MouseInputData
	{
		public int X;
		public int Y;
		public int ScrollWheelValue;
		public int HorizontalScrollWheelValue;
		public bool LeftButton;
		public bool RightButton;
		public bool MiddleButton;
		public bool XButton1;
		public bool XButton2;

		public static MouseInputData FromMouseState(MouseState state)
		{
			return new MouseInputData
			{
				X = state.X,
				Y = state.Y,
				ScrollWheelValue = state.ScrollWheelValue,
				HorizontalScrollWheelValue = state.HorizontalScrollWheelValue,
				LeftButton = state.LeftButton == ButtonState.Pressed,
				RightButton = state.RightButton == ButtonState.Pressed,
				MiddleButton = state.MiddleButton == ButtonState.Pressed,
				XButton1 = state.XButton1 == ButtonState.Pressed,
				XButton2 = state.XButton2 == ButtonState.Pressed
			};
		}

		public MouseState ToMouseState()
		{
			return new MouseState(
				X,
				Y,
				ScrollWheelValue,
				LeftButton ? ButtonState.Pressed : ButtonState.Released,
				MiddleButton ? ButtonState.Pressed : ButtonState.Released,
				RightButton ? ButtonState.Pressed : ButtonState.Released,
				XButton1 ? ButtonState.Pressed : ButtonState.Released,
				XButton2 ? ButtonState.Pressed : ButtonState.Released,
				HorizontalScrollWheelValue
			);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct KeyboardInputData
	{
		public Keys[] PressedKeys;
		public bool CapsLock;
		public bool NumLock;

		public static KeyboardInputData FromKeyboardState(KeyboardState state)
		{
			return new KeyboardInputData
			{
				PressedKeys = state.GetPressedKeys(),
				CapsLock = state.CapsLock,
				NumLock = state.NumLock
			};
		}

		public KeyboardState ToKeyboardState()
			=> new KeyboardState(PressedKeys, CapsLock, NumLock);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Serializable]
	[MessagePackObject(keyAsPropertyName: true)]
	public struct GamePadInputData
	{
		public bool IsConnected;
		public int PacketNumber;

		// Buttons
		public bool A;
		public bool B;
		public bool X;
		public bool Y;
		public bool Back;
		public bool Start;
		public bool LeftShoulder;
		public bool RightShoulder;
		public bool LeftStick;
		public bool RightStick;
		public bool BigButton;

		// DPad
		public bool DPadUp;
		public bool DPadDown;
		public bool DPadLeft;
		public bool DPadRight;

		// Thumbsticks
		public float LeftThumbstickX;
		public float LeftThumbstickY;
		public float RightThumbstickX;
		public float RightThumbstickY;

		// Triggers
		public float LeftTrigger;
		public float RightTrigger;

		public static GamePadInputData FromGamePadState(GamePadState state)
		{
			return new GamePadInputData
			{
				IsConnected = state.IsConnected,
				PacketNumber = state.PacketNumber,
				A = state.Buttons.A == ButtonState.Pressed,
				B = state.Buttons.B == ButtonState.Pressed,
				X = state.Buttons.X == ButtonState.Pressed,
				Y = state.Buttons.Y == ButtonState.Pressed,
				Back = state.Buttons.Back == ButtonState.Pressed,
				Start = state.Buttons.Start == ButtonState.Pressed,
				LeftShoulder = state.Buttons.LeftShoulder == ButtonState.Pressed,
				RightShoulder = state.Buttons.RightShoulder == ButtonState.Pressed,
				LeftStick = state.Buttons.LeftStick == ButtonState.Pressed,
				RightStick = state.Buttons.RightStick == ButtonState.Pressed,
				BigButton = state.Buttons.BigButton == ButtonState.Pressed,
				DPadUp = state.DPad.Up == ButtonState.Pressed,
				DPadDown = state.DPad.Down == ButtonState.Pressed,
				DPadLeft = state.DPad.Left == ButtonState.Pressed,
				DPadRight = state.DPad.Right == ButtonState.Pressed,
				LeftThumbstickX = state.ThumbSticks.Left.X,
				LeftThumbstickY = state.ThumbSticks.Left.Y,
				RightThumbstickX = state.ThumbSticks.Right.X,
				RightThumbstickY = state.ThumbSticks.Right.Y,
				LeftTrigger = state.Triggers.Left,
				RightTrigger = state.Triggers.Right
			};
		}

		public GamePadState ToGamePadState()
		{
			return new GamePadState(
				new GamePadThumbSticks(
					new Microsoft.Xna.Framework.Vector2(LeftThumbstickX, LeftThumbstickY),
					new Microsoft.Xna.Framework.Vector2(RightThumbstickX, RightThumbstickY)
				),
				new GamePadTriggers(LeftTrigger, RightTrigger),
				new GamePadButtons(
					(A ? Buttons.A : 0) |
					(B ? Buttons.B : 0) |
					(X ? Buttons.X : 0) |
					(Y ? Buttons.Y : 0) |
					(Back ? Buttons.Back : 0) |
					(Start ? Buttons.Start : 0) |
					(LeftShoulder ? Buttons.LeftShoulder : 0) |
					(RightShoulder ? Buttons.RightShoulder : 0) |
					(LeftStick ? Buttons.LeftStick : 0) |
					(RightStick ? Buttons.RightStick : 0) |
					(BigButton ? Buttons.BigButton : 0)
				),
				new GamePadDPad(
					DPadUp ? ButtonState.Pressed : ButtonState.Released,
					DPadDown ? ButtonState.Pressed : ButtonState.Released,
					DPadLeft ? ButtonState.Pressed : ButtonState.Released,
					DPadRight ? ButtonState.Pressed : ButtonState.Released
				)
			);
		}
	}
}