using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonogameTest1
{
	enum AnimalType
	{
		Dog,
		Horse,
		Rooster,
	}

	class AnimalConfig
	{

	}

	interface IBehaviour
	{
		bool ShouldRun();
		void ExecuteBehaviour(GameTime gameTime, IEntity owner);
	}

	class FollowBehaviour : IBehaviour
	{
		public bool ShouldRun() => Target != null;
		public IEntity Target { get; set; }

		public int TargetDistanceTolerance = 32;
		bool atTarget = true;

		public void ExecuteBehaviour(GameTime gameTime, IEntity owner)
		{
			// follow target
			var distanceFromTarget = (Target.Position - owner.Position).Length();

			// triggers start of move to target
			if (atTarget && distanceFromTarget > TargetDistanceTolerance)
			{
				atTarget = false;
			}

			// move to target
			if (!atTarget)
			{
				var dir = Target.Position - owner.Position;
				dir.Normalize();
				var amount = MathHelper.Clamp(distanceFromTarget, 0, TargetDistanceTolerance) / TargetDistanceTolerance;
				var distanceModifier = MathHelper.SmoothStep(0f, 1f, amount);
				owner.Acceleration += dir * owner.GetAcceleration() * distanceModifier;
			}

			// we're at target, stop
			if (distanceFromTarget < owner.GetAcceleration())
			{
				atTarget = true;
			}
		}
	}

	class Animal : IEntity
	{
		// IKinematics
		public Vector2 _position;
		public Vector2 _velocity;
		public Vector2 _acceleration;
		public Vector2 Position { get => _position; set => _position = value; }
		public Vector2 Velocity { get => _velocity; set => _velocity = value; }
		public Vector2 Acceleration { get => _acceleration; set => _acceleration = value; }

		public Vector2 Size = new(48, 48);
		public string Name;

		public Vector2 Direction;
		//public Vector2 OldPosition;
		public float AccelerationSpeed;
		//public Vector2 TargetPosition;
		public AnimalType AnimalType;

		public List<IBehaviour> Behaviours = new();

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
				Position - (Size / 2),
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
		public Vector2 GetPosition() => Position;
		public Vector2 GetSize() => Size;
		public string GetName() => Name;

		public float GetAcceleration() => AccelerationSpeed;

		public void SetPosition(Vector2 pos) => Position = pos;
	}
}
