using Microsoft.Xna.Framework;

namespace Odyssey.World
{
	public class Map
	{
		public int Width;
		public int Height;
		public int TileSize;
		private Tile[,] Data;
		public int[,] Trees; // -1 = no tree, 0->infinity = tree id

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

			//Trees[10, 10] = 1;
		}

		public Tile At(int x, int y) => Data[x, y];

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
	}
}
