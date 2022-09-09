using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Odyssey.World
{
	internal class Map
	{
		public int Width;
		public int Height;
		public int TileSize;
		private Tile[,] Data;
		private int[,] Trees; // -1 = no tree, 0->infinity = tree id

		public bool DrawNoiseOnly = true;
		public bool UseColourMap = false;

		public Map(int width, int height) => InitialiseMap(width, height);

		private void InitialiseMap(int width, int height)
		{
			Width = width;
			Height = height;
			Data = new Tile[Width, Height];
			Trees = new int[Width, Height];

			// init
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					Data[x, y] = new Tile
					{
						UseColourMap = UseColourMap
					};
					Trees[x, y] = -1;
				}
			}

			Trees[10, 10] = 1;
		}

		public Map(int width, int height, double[] data) : this(width, height) => SetData(data);

		public Map(int width, int height, double[,] data) : this(width, height) => SetData(data);
		public Map(double[,] data) => SetData(data);

		public void SetData(double[,] data)
		{
			if (data.Length != Width * Height)
			{
				var newWidth = data.GetLength(0);
				var newHeight = data.GetLength(1);
				InitialiseMap(newWidth, newHeight);
			}

			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					Data[x, y].Value = data[x, y];
				}
			}
		}

		public void SetData(double[] data)
		{
			if (data.Length != Width * Height)
			{
				throw new ArgumentOutOfRangeException("data doesn't match map size");
			}

			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					Data[x, y].Value = data[y * Height + x];
				}
			}
		}

		public static void Update(GameTime gameTime)
		{
		}

		public void Draw(SpriteBatch sb, GameTime gameTime, Camera camera)
		{
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					if (DrawNoiseOnly)
					{
						sb.Draw(GameServices.Textures["pixel"], new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize), null, Data[x, y].Colour);
					}
					else
					{
						DrawTile(sb, camera, y, x);
					}
				}
			}
		}

		private void DrawTile(SpriteBatch sb, Camera camera, int y, int x)
		{
			var srcRect = Data[x, y].MapRect;
			var dstRect = new Rectangle(x * TileSize, y * TileSize, TileSize, TileSize);

			var safeCamera = new Rectangle(
				camera.VisibleArea.X - TileSize,
				camera.VisibleArea.Y - TileSize,
				camera.VisibleArea.Width + 2 * TileSize,
				camera.VisibleArea.Height + 2 * TileSize);

			if (safeCamera.Contains(dstRect))
			{
				//sb.Draw(GameServices.Textures["terrain"], dstRect, srcRect, Data[x, y].Colour);
				var grey = (float)(Data[x, y].value / 2.0 + 0.5);
				sb.Draw(GameServices.Textures["terrain"], dstRect, srcRect, new Color(grey, grey, grey));
			}

			if (Trees[x, y] != -1)
			{
				var treeSrcRect = new Rectangle(0, 160, 128, 160);
				var treeOrigin = new Point(treeSrcRect.Width / 2, treeSrcRect.Height);
				var treeDstRect = new Rectangle((x * TileSize) - treeOrigin.X, (y * TileSize) - treeOrigin.Y, treeSrcRect.Width, treeSrcRect.Height);
				if (safeCamera.Contains(treeDstRect))
				{
					sb.Draw(GameServices.Textures["grassland"], treeDstRect, treeSrcRect, Color.White);
					sb.DrawRectangle(treeDstRect, Color.Green, 2);
				}
			}
		}
	}


	public enum TileType
	{
		Water, Sand, Grass, Forest, Mountain, Snow
	}
}
