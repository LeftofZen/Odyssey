using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonogameTest1
{
	public interface IEntityComponent
	{
		int ID { get; set; }
	}

	public abstract class EntityComponent : IEntityComponent
	{
		public EntityComponent(int id) => ID = id;

		public int ID { get; set; }
	}

	public abstract class Entity : EntityComponent
	{
		public Entity(int id) : base(id) { }
	}

	public abstract class Component : EntityComponent
	{
		public Component(int id) : base(id) { }
	}


	public interface ISystem
	{
		//void Update(GameTime gameTime, List<IComponent> components, List<IEntity> entities);
	}

	public class KinematicsSystem : ISystem
	{
		public void Update(GameTime gameTime, List<IEntityComponent> transforms, List<IEntityComponent> kinematics)
		{
			foreach (KinematicsComponent v in kinematics)
			{
				var t = transforms.Where(a => a.ID == v.ID).Cast<TransformComponent>().SingleOrDefault();

				t.Position += v.Velocity;
				v.Velocity += v.Acceleration;
			}
		}
	}

	public class TransformComponent : Component
	{
		public TransformComponent(int id) : base(id) { }
		public Vector2 Position { get; set; }
		public float Rotation { get; set; }
		public float Scale { get; set; }
	}
	public class KinematicsComponent : Component
	{
		public KinematicsComponent(int id) : base(id) { }
		public Vector2 Velocity { get; set; }
		public Vector2 Acceleration { get; set; }
	}


	public class PlayerEntity : Entity
	{
		public PlayerEntity(int id) : base(id)
		{

		}
	}
}
