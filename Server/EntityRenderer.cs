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
			sb.FillRectangle(new RectangleF(entity.Position.X * scale, entity.Position.X * scale, entity.GetSize().X * scale, entity.GetSize().Y * scale), Color.Chocolate);
			sb.DrawDebugStringCentered(GameServices.Fonts.First().Value, entity.DisplayName, entity.Position, Color.White);
		}
	}
}