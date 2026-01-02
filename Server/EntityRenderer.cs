using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Odyssey.ECS;
using Odyssey.Render;
using System.Linq;

namespace Odyssey.Server
{
	public static class EntityRenderer
	{
		public static void Draw(SpriteBatch sb, IEntity entity, float scale)
		{
			// minimap
			sb.FillRectangle(new RectangleF(entity.Position.X * scale, entity.Position.Y * scale, entity.GetSize().X * scale, entity.GetSize().Y * scale), Color.Chocolate);

			// player
			var stringPos = entity.Position + new Vector2(entity.GetSize().X / 2f, -8);
			sb.DrawDebugStringCentered(GameServices.Fonts.First().Value, entity.DisplayName, stringPos, Color.White);
			sb.DrawRectangle(new RectangleF(entity.Position, entity.GetSize()), Color.Blue);
		}
	}
}