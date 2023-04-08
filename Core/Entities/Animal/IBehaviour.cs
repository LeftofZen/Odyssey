using Microsoft.Xna.Framework;
using Odyssey;
using Odyssey.ECS;

namespace Odyssey.Entities.Animal
{
	public interface IBehaviour
	{
		bool ShouldRun();
		void ExecuteBehaviour(GameTime gameTime, IEntity owner);
	}
}
