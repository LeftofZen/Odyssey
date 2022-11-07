using Microsoft.Xna.Framework;

namespace Odyssey
{
	public interface IEntity : IKinematics
	{
		Guid Id { get; set; }
		Vector2 GetSize();

		string DisplayName { get; }
	}
}
