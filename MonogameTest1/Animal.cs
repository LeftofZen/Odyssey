using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonogameTest1
{
	class Animal
	{
		public Vector2 Position;
		public Vector2 Direction;
		public float MoveSpeed;
		public Texture2D Texture;
		public Vector2 TargetPosition;

		Point tilesize = new Point(48, 48);

		public void Update(GameTime gameTime)
		{
			if ((TargetPosition-Position).Length() > MoveSpeed)
			{
				var dir = TargetPosition - Position;
				dir.Normalize();
				Position += dir * MoveSpeed;
			}
		}

		public void Draw(SpriteBatch sb, GameTime gameTime)
		{
			sb.Draw(Texture,
				Position - new Vector2(tilesize.X / 2, tilesize.Y / 2),
				new Rectangle(0, 0, tilesize.X, tilesize.Y),
				Color.White);
		}
	}
}
