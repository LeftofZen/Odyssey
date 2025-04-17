using Microsoft.Xna.Framework;

namespace Odyssey.ECS
{
	public interface IEntity : IKinematics
	{
		Guid Id { get; set; }
		Vector2 GetSize();

		ICollection<IComponent> Components { get;  }

		string DisplayName { get; }
	}
}
