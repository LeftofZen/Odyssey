using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Odyssey.ECS;
using Serilog;

namespace Odyssey.Entities.Animal
{
	public class Animal : IEntity
	{
		// IKinematics
		public Vector2 _position;
		public Vector2 _velocity;
		public Vector2 _acceleration;
		public Vector2 Position { get => _position; set => _position = value; }
		public Vector2 Velocity { get => _velocity; set => _velocity = value; }
		public Vector2 Acceleration { get => _acceleration; set => _acceleration = value; }

		public Vector2 Size = new(48, 48);
		public string DisplayName { get; set; }

		public Vector2 Direction;
		//public Vector2 OldPosition;
		public float AccelerationSpeed;
		//public Vector2 TargetPosition;
		public AnimalType AnimalType;

		public List<IBehaviour> Behaviours = new();

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

		public void Update(GameTime gameTime)
		{
			foreach (var behaviour in Behaviours.Where(b => b.ShouldRun()))
			{
				behaviour.ExecuteBehaviour(gameTime, this);
			}

			UpdateKinematics(gameTime);

			//var distanceFromTarget = (TargetPosition - Position).Length();

			//// triggers start of move to target
			//if (AtTarget && distanceFromTarget > MoveSpeed * TargetDistanceTolerance)
			//{
			//	AtTarget = false;
			//}

			//// move to target
			//if (!AtTarget)
			//{
			//	Direction = TargetPosition - Position;
			//	Direction.Normalize();
			//	Position += Direction * MoveSpeed;

			//	// stuff here happens only when animal hasn't reached the target
			//	if ((int)(gameTime.TotalGameTime.TotalMilliseconds) % 2000 == 0)
			//	{
			//		SoundEffectInstance animalSound;

			//		switch (AnimalType)
			//		{
			//			case AnimalType.Dog:
			//				animalSound = GameServices.SoundEffects["dogbark"].CreateInstance();
			//				break;
			//			case AnimalType.Horse:
			//				animalSound = GameServices.SoundEffects["ponywhinny"].CreateInstance();
			//				break;
			//			case AnimalType.Rooster:
			//				animalSound = GameServices.SoundEffects["rooster"].CreateInstance();
			//				break;
			//			default:
			//				animalSound = GameServices.SoundEffects["rooster"].CreateInstance();
			//				break;
			//		}

			//		animalSound.Volume = 0.1f;
			//		animalSound.Play();

			//	}
			//}

			//// we're at target, stop
			//if (distanceFromTarget < MoveSpeed)
			//{
			//	AtTarget = true;
			//}
		}
		public void UpdateKinematics(GameTime gameTime)
		{
			Velocity += Acceleration;
			Position += Velocity;

			Acceleration = Vector2.Zero; // arresting
			Velocity *= 0.95f; // damping
		}

		public void Draw(SpriteBatch sb, GameTime gameTime)
		{
			sb.Draw(
				GameServices.Textures["animals"],
				Position - Size / 2,
				new Rectangle(0, 0, (int)Size.X, (int)Size.Y),
				Color.White);

			// directional arrow
			var dir = Velocity;
			if (dir.Length() != 0)
			{
				dir.Normalize();
				sb.Draw(
					GameServices.Textures["ui"],
					Position,
					new Rectangle(32 * 2, 0, 32, 32),
					Color.White,
					(float)Math.Atan2(dir.Y, dir.X),
					new Vector2(16),
					1f,
					SpriteEffects.None, 0f);
			}

			//var target = Behaviours.OfType<FollowBehaviour>().SingleOrDefault().Target.Position;
			////target directional arrow
			//sb.Draw(
			//	GameServices.Textures["ui"],
			//	target - new Vector2(16),
			//	new Rectangle(32 * 3, 0, 32, 32),
			//	Color.White,
			//	0f,
			//	Vector2.Zero,
			//	1f,
			//	SpriteEffects.None, 0f);
		}

		public Vector2 GetSize() => Size;

		public ICollection<IComponent> Components => throw new NotImplementedException();
	}
}
