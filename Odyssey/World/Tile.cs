using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Odyssey.World
{
	internal class Tile
	{
		// to port from noise map
		public double Value
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
					Colour = new Color((float)value, (float)value, (float)value);
				}
			}
		}
		public double value;

		public bool UseColourMap { get; set; }

		public Rectangle MapRect => SpriteMap[Map(value)];

		public static TileType Map(double value)
		{
			if (value < 0.4)
			{
				return TileType.Mountain;
			}

			if (value < 0.5)
			{
				return TileType.Forest;
			}

			if (value < 0.6)
			{
				return TileType.Grass;
			}

			if (value < 0.7)
			{
				return TileType.Sand;
			}

			if (value < 1.0)
			{
				return TileType.Water;
			}

			return TileType.Snow;
		}

		public Color Colour;
		private static readonly Dictionary<TileType, Color> ColourMap = new()
		{
			{ TileType.Water, Color.Blue },
			{ TileType.Sand, Color.DarkGoldenrod },
			{ TileType.Grass, Color.LightGreen },
			{ TileType.Forest, Color.DarkGreen },
			{ TileType.Mountain, Color.Gray },
			{ TileType.Snow, Color.LightGray },
		};
		private static readonly Dictionary<TileType, Rectangle> SpriteMap = new()
		{
			{ TileType.Water, new Rectangle(608, 96, 32, 32) },
			{ TileType.Sand, new Rectangle(32, 480, 32, 32) },
			{ TileType.Grass, new Rectangle(64, 352, 32, 32) },
			{ TileType.Forest, new Rectangle(224, 352, 32, 32) },
			{ TileType.Mountain, new Rectangle(416, 480, 32, 32) },
			{ TileType.Snow, new Rectangle(0, 32, 32, 32) },
		};
	}
}
