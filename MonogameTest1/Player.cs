using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonogameTest1
{
	class Player : IEntity
	{
		public Vector2 Position;
		public Vector2 Size = new Vector2(24, 32);
		public Vector2 Direction;
		public float MoveSpeed;
		public string Name;

		public void Update(GameTime gameTime)
		{
			var speedModifier = 1f;

			if (Keyboard.GetState().IsKeyDown(Keys.RightShift))
			{
				speedModifier = 4f;
			}

			var speed = MoveSpeed * speedModifier;

			if (Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				Position.Y += -speed;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Down))
			{
				Position.Y += speed;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Left))
			{
				Position.X += -speed;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Right))
			{
				Position.X += speed;
			}
		}

		public void Draw(SpriteBatch sb, GameTime gameTime)
		{
			sb.Draw(GameServices.Textures["char"],
				Position - new Vector2(Size.X / 2, Size.Y / 2),
				new Rectangle(72, 64, (int)Size.X, (int)Size.Y),
				Color.White);
		}

		public Vector2 GetPosition() => Position;
		public Vector2 GetSize() => Size;

		public string GetName() => Name;
	}
}
