using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Odyssey.Entities
{
	internal class Player : IEntity
	{
		// IKinematics
		public Vector2 _position;
		public Vector2 _velocity;
		public Vector2 _acceleration;
		public Vector2 Position { get => _position; set => _position = value; }
		public Vector2 Velocity { get => _velocity; set => _velocity = value; }
		public Vector2 Acceleration { get => _acceleration; set => _acceleration = value; }

		public Vector2 Size = new(24, 32);
		public Vector2 Direction;
		public float MoveSpeed;
		public string Name;

		public void Update(NetworkInput input, GameTime gameTime)
		{
			UpdateKinematics(gameTime);

			var speedModifier = 1f;

			if (input.Keyboard.IsKeyDown(Keys.RightShift))
			{
				speedModifier = 4f;
			}

			var speed = MoveSpeed * speedModifier;

			if (input.Keyboard.IsKeyDown(Keys.Up))
			{
				_position.Y += -speed;
			}

			if (input.Keyboard.IsKeyDown(Keys.Down))
			{
				_position.Y += speed;
			}

			if (input.Keyboard.IsKeyDown(Keys.Left))
			{
				_position.X += -speed;
			}

			if (input.Keyboard.IsKeyDown(Keys.Right))
			{
				_position.X += speed;
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
		public float GetAcceleration() => MoveSpeed;
		public void SetPosition(Vector2 pos) => Position = pos;

		public void UpdateKinematics(GameTime gameTime)
		{
			Velocity += Acceleration;
			Position += Velocity;

			Acceleration = Vector2.Zero; // arresting
			Velocity *= 0.95f; // damping
		}
	}
}
