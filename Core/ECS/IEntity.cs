using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
