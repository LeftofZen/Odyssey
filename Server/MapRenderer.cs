using MessagePack;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Odyssey.World;

namespace Odyssey.Server
{
	public static class MapRenderer
	{
		public static void Draw(SpriteBatch sb, Map map, int xOffset, int yOffset, int tileSize = 32)
		{
			for (var y = 0; y < map.Height; y++)
			{
				for (var x = 0; x < map.Width; x++)
				{
					sb.FillRectangle(new RectangleF(xOffset + (x * tileSize), yOffset + (y * tileSize), tileSize, tileSize), map.At(x, y).Colour);
				}
			}
		}
	}
}