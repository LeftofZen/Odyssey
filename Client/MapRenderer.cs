using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Odyssey.World;

namespace Odyssey.Client
{
	public static class MapRenderer
	{
		public static void Draw(this Map map, SpriteBatch sb, GameTime gameTime, Camera camera)
		{
			for (var y = 0; y < map.Height; y++)
			{
				for (var x = 0; x < map.Width; x++)
				{
					if (map.DrawNoiseOnly)
					{
						sb.Draw(GameServices.Textures["pixel"], new Rectangle(x * map.TileSize, y * map.TileSize, map.TileSize, map.TileSize), null, map.At(x, y).Colour);
					}
					else
					{
						map.DrawTile(sb, camera, y, x);
					}
				}
			}
		}

		public static void DrawTile(this Map map, SpriteBatch sb, Camera camera, int y, int x)
		{
			var srcRect = map.At(x, y).MapRect;
			var dstRect = new Rectangle(x * map.TileSize, y * map.TileSize, map.TileSize, map.TileSize);

			var safeCamera = new Rectangle(
				camera.VisibleArea.X - map.TileSize,
				camera.VisibleArea.Y - map.TileSize,
				camera.VisibleArea.Width + 2 * map.TileSize,
				camera.VisibleArea.Height + 2 * map.TileSize);

			if (safeCamera.Contains(dstRect))
			{
				//sb.Draw(GameServices.Textures["terrain"], dstRect, srcRect, Data[x, y].Colour);
				var grey = (float)(map.At(x, y).value / 2.0 + 0.5);
				sb.Draw(GameServices.Textures["terrain"], dstRect, srcRect, new Color(grey, grey, grey));
			}

			if (map.Trees[x, y] != -1)
			{
				var treeSrcRect = new Rectangle(0, 160, 128, 160);
				var treeOrigin = new Point(treeSrcRect.Width / 2, treeSrcRect.Height);
				var treeDstRect = new Rectangle((x * map.TileSize) - treeOrigin.X, (y * map.TileSize) - treeOrigin.Y, treeSrcRect.Width, treeSrcRect.Height);
				if (safeCamera.Contains(treeDstRect))
				{
					sb.Draw(GameServices.Textures["grassland"], treeDstRect, treeSrcRect, Color.White);
					sb.DrawRectangle(treeDstRect, Color.Green, 2);
				}
			}
		}
	}
}
