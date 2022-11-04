using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Odyssey.Networking.Messages;
using Serilog;

namespace Odyssey.Entities
{
	public class Player : IEntity
	{
		// IKinematics
		private Vector2 _position;
		private Vector2 _velocity;
		private Vector2 _acceleration;
		public Vector2 Position { get => _position; set => _position = value; }
		public Vector2 Velocity { get => _velocity; set => _velocity = value; }
		public Vector2 Acceleration { get => _acceleration; set => _acceleration = value; }

		public Vector2 Size = new(24, 32);
		public Vector2 Direction;
		public float MoveSpeed;

		public string DisplayName { get; set; }

		#region Server Auth

		public string Username { get; set; }

		public string Password { get; set; }

		// This is a server-assigned id sent to the client upon login that the client should use for all messages
		public Guid Id
		{
			get => id;
			set
			{
				if (id != value)
				{
					Log.Debug("[Player::Id_Set] {id}", value);
					id = value;
				}
			}
		}
		private Guid id;

		#endregion

		public void Update(InputUpdate input, GameTime gameTime)
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

		public void Draw(SpriteBatch sb, GameTime gameTime) =>
			//sb.Draw(GameServices.Textures["char"],
			//	Position - new Vector2(Size.X / 2, Size.Y / 2),
			//	new Rectangle(72, 64, (int)Size.X, (int)Size.Y),
			//	Color.White);
			sb.DrawRectangle(new RectangleF(Position.X, Position.Y, 32, 32), Color.White);

		public Vector2 GetPosition() => Position;
		public Vector2 GetSize() => Size;

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
