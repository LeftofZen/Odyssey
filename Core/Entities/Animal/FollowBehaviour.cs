using Microsoft.Xna.Framework;
using Odyssey.ECS;

namespace Odyssey.Entities.Animal
{
	public class FollowBehaviour : IBehaviour
	{
		public bool ShouldRun() => Target != null;
		public IEntity Target { get; set; }

		public int TargetDistanceTolerance = 32;
		private bool atTarget = true;

		public void ExecuteBehaviour(GameTime gameTime, IEntity owner)
		{
			// follow target
			var distanceFromTarget = (Target.Position - owner.Position).Length();

			// triggers start of move to target
			if (atTarget && distanceFromTarget > TargetDistanceTolerance)
			{
				atTarget = false;
			}

			// move to target
			if (!atTarget)
			{
				var dir = Target.Position - owner.Position;
				dir.Normalize();
				var amount = MathHelper.Clamp(distanceFromTarget, 0, TargetDistanceTolerance) / TargetDistanceTolerance;
				var distanceModifier = MathHelper.SmoothStep(0f, 1f, amount);
				owner.Acceleration += dir * owner.Acceleration * distanceModifier;
			}

			// we're at target, stop
			if (distanceFromTarget < owner.Acceleration.Length())
			{
				atTarget = true;
			}
		}
	}
}
