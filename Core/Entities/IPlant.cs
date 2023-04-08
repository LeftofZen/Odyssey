using Odyssey.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Entities
{
	public interface IRoot
	{ }

	public interface ILeaf
	{ }

	public interface ITrunk
	{ }

	public interface IBranch
	{ }

	public interface IFlower
	{ }

	public struct RootComponent : IRoot, IComponent
	{ }

	public interface IPlant : IEntity
	{
		void Grow();

		public IEnumerable<IRoot> Roots => Components.OfType<IRoot>();
	}
}
