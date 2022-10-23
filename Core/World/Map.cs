using Microsoft.Xna.Framework;

namespace Odyssey.World
{
	[Serializable]
	public struct Map
	{
		public int Width { get; set; } = 0;
		public int Height { get; set; } = 0;
		public int TileSize { get; set; } = 0;
		private Tile[,] Data { get; set; } = null;
		public int[,] Trees = null; // -1 = no tree, 0->infinity = tree id

		public bool DrawNoiseOnly = true;
		public bool UseColourMap = false;

		public bool IsInitialised = false;

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

			IsInitialised = true;
		}

		public Tile At(int x, int y) => Data[x, y];

		public Map(int width, int height) => InitialiseMap(width, height);

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
