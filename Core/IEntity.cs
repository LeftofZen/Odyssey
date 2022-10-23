using Microsoft.Xna.Framework;

namespace Odyssey
{
	public interface IEntity : IKinematics
	{
		//Vector2 GetPosition();
		Vector2 GetSize();

		float GetAcceleration();

		string GetName();

		//void SetPosition(Vector2 pos);
	}
}
