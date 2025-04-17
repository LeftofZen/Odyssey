using Odyssey.ECS;
using Odyssey.World;

namespace Odyssey
{
	public struct GameState
	{
		public Map Map { get; set; }

		// this should essentially be keyed off IEntity.Id, so could be Dictionary
		public List<IEntity> Entities { get; set; }
	}
}
