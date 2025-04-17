using Microsoft.Xna.Framework;

namespace Odyssey
{
	public interface IKinematics
	{
		Vector2 Position { get; set; }
		Vector2 Velocity { get; set; }
		Vector2 Acceleration { get; set; }

		void UpdateKinematics(GameTime gameTime);
	}
}
