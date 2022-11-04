using Microsoft.Xna.Framework;
using Odyssey;

namespace Odyssey.Entities.Animal
{
	public interface IBehaviour
	{
		bool ShouldRun();
		void ExecuteBehaviour(GameTime gameTime, IEntity owner);
	}
}
