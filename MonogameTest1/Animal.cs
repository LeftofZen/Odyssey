using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonogameTest1
{
	enum AnimalType
	{
		Dog,
		Horse,
		Rooster,
	}

	class Animal : IEntity
	{
		public Vector2 Position;
		public Vector2 Size = new Vector2(48, 48);
		public string Name;

		public Vector2 Direction;
		public float MoveSpeed;
		public Vector2 TargetPosition;
		public AnimalType AnimalType;

		public bool AtTarget = true;
		public int TargetDistanceTolerance = 2;


		public void Update(GameTime gameTime)
		{
			var distanceFromTarget = (TargetPosition - Position).Length();

			// triggers start of move to target
			if (AtTarget && distanceFromTarget > MoveSpeed * TargetDistanceTolerance)
			{
				AtTarget = false;
			}

			// move to target
			if (!AtTarget)
			{
				Direction = TargetPosition - Position;
				Direction.Normalize();
				Position += Direction * MoveSpeed;

				// stuff here happens only when animal hasn't reached the target
				if ((int)(gameTime.TotalGameTime.TotalMilliseconds) % 2000 == 0)
				{
					SoundEffectInstance animalSound;

					switch (AnimalType)
					{
						case AnimalType.Dog:
							animalSound = GameServices.SoundEffects["dogbark"].CreateInstance();
							break;
						case AnimalType.Horse:
							animalSound = GameServices.SoundEffects["ponywhinny"].CreateInstance();
							break;
						case AnimalType.Rooster:
							animalSound = GameServices.SoundEffects["rooster"].CreateInstance();
							break;
						default:
							animalSound = GameServices.SoundEffects["rooster"].CreateInstance();
							break;
					}

					animalSound.Volume = 0.1f;
					animalSound.Play();

				}
			}

			// we're at target, stop
			if (distanceFromTarget < MoveSpeed)
			{
				AtTarget = true;
			}
		}

		public void Draw(SpriteBatch sb, GameTime gameTime)
		{
			sb.Draw(
				GameServices.Textures["animals"],
				Position - (Size / 2),
				new Rectangle(0, 0, (int)Size.X, (int)Size.Y),
				Color.White);

			// directional arrow
			sb.Draw(
				GameServices.Textures["ui"],
				Position,
				new Rectangle(32 * 2, 0, 32, 32),
				Color.White,
				(float)Math.Atan2(Direction.Y, Direction.X),
				new Vector2(16),
				1f,
				SpriteEffects.None, 0f);

			// target location
			sb.Draw(
				GameServices.Textures["ui"],
				TargetPosition - new Vector2(16),
				new Rectangle(32 * 3, 0, 32, 32),
				Color.White,
				0f,
				Vector2.Zero,
				1f,
				SpriteEffects.None, 0f);
		}
		public Vector2 GetPosition() => Position;
		public Vector2 GetSize() => Size;
		public string GetName() => Name;
	}
}
