using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonogameTest1
{
	class Player
	{
		public Vector2 Position;
		public Vector2 Direction;
		public float MoveSpeed;
		public Texture2D Texture;

		Point tilesize = new Point(24, 32);

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
			sb.Draw(Texture,
				Position - new Vector2(tilesize.X / 2, tilesize.Y / 2),
				new Rectangle(72, 64, tilesize.X, tilesize.Y),
				Color.White);
		}
	}
}
