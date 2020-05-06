using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MonogameTest1
{
	class Map
	{
		public int Width;
		public int Height;
		public int TileSize;

		Tile[,] Data;


		public bool DrawNoiseOnly = true;
		public bool UseColourMap = false;

		public Map(int width, int height)
		{
			InitialiseMap(width, height);
		}

		void InitialiseMap(int width, int height)
		{
			Width = width;
			Height = height;
			Data = new Tile[Width, Height];

			// init
			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width; x++)
				{
					Data[x, y] = new Tile();
					Data[x, y].UseColourMap = this.UseColourMap;
				}
			}
		}

		public Map(int width, int height, float[] data) : this(width, height)
		{
			SetData(data);
		}

		public Map(int width, int height, float[,] data) : this(width, height)
		{
			SetData(data);
		}
		public Map(float[,] data)
		{
			SetData(data);
		}

		public void SetData(float[,] data)
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

		public void SetData(float[] data)
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

		public void Update(GameTime gameTime)
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
							var grey = Data[x, y].value / 2f + 0.5f;
							sb.Draw(GameServices.Textures["terrain"], dstRect, srcRect, new Color(grey, grey, grey));
						}
					}
				}
			}
		}



	}

	class Tile
	{
		// to port from noise map
		public float Value
		{
			set
			{
				this.value = value;
				if (UseColourMap)
				{
					Colour = ColourMap[Map(value)];
				}
				else
				{
					Colour = new Color(value, value, value);
				}
			}
		}
		public float value;

		public bool UseColourMap { get; set; } = false;

		public Rectangle MapRect => SpriteMap[Map(value)];

		public TileType Map(float value)
		{
			if (value < 0.4f) return TileType.Mountain;
			if (value < 0.5f) return TileType.Forest;
			if (value < 0.6f) return TileType.Grass;
			if (value < 0.7f) return TileType.Sand;
			if (value < 1.0f) return TileType.Water;
			return TileType.Snow;
		}

		public Color Colour;

		static Dictionary<TileType, Color> ColourMap = new Dictionary<TileType, Color>
		{
			{ TileType.Water, Color.Blue },
			{ TileType.Sand, Color.DarkGoldenrod },
			{ TileType.Grass, Color.LightGreen },
			{ TileType.Forest, Color.DarkGreen },
			{ TileType.Mountain, Color.Gray },
			{ TileType.Snow, Color.LightGray },
		};

		static Dictionary<TileType, Rectangle> SpriteMap = new Dictionary<TileType, Rectangle>
		{
			{ TileType.Water, new Rectangle(608, 96, 32, 32) },
			{ TileType.Sand, new Rectangle(32, 480, 32, 32) },
			{ TileType.Grass, new Rectangle(64, 352, 32, 32) },
			{ TileType.Forest, new Rectangle(224, 352, 32, 32) },
			{ TileType.Mountain, new Rectangle(416, 480, 32, 32) },
			{ TileType.Snow, new Rectangle(0, 32, 32, 32) },
		};
	}

	public enum TileType
	{
		Water, Sand, Grass, Forest, Mountain, Snow
	}
}
