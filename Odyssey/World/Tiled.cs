using System.Collections.Generic;

namespace Odyssey
{
	namespace Tiled
	{
		public class Layer
		{
			public List<int> data { get; set; }
			public int height { get; set; }
			public int id { get; set; }
			public string name { get; set; }
			public int opacity { get; set; }
			public string type { get; set; }
			public bool visible { get; set; }
			public int width { get; set; }
			public int x { get; set; }
			public int y { get; set; }
		}

		public class Terrain
		{
			public string name { get; set; }
			public int tile { get; set; }
		}

		public class Tile
		{
			public int id { get; set; }
			public List<int> terrain { get; set; }
		}

		public class Tileset
		{
			public int columns { get; set; }
			public int firstgid { get; set; }
			public string image { get; set; }
			public int imageheight { get; set; }
			public int imagewidth { get; set; }
			public int margin { get; set; }
			public string name { get; set; }
			public int spacing { get; set; }
			public List<Terrain> terrains { get; set; }
			public int tilecount { get; set; }
			public int tileheight { get; set; }
			public List<Tile> tiles { get; set; }
			public int tilewidth { get; set; }
		}

		public class Map
		{
			public int compressionlevel { get; set; }
			public int height { get; set; }
			public bool infinite { get; set; }
			public List<Layer> layers { get; set; }
			public int nextlayerid { get; set; }
			public int nextobjectid { get; set; }
			public string orientation { get; set; }
			public string renderorder { get; set; }
			public string tiledversion { get; set; }
			public int tileheight { get; set; }
			public List<Tileset> tilesets { get; set; }
			public int tilewidth { get; set; }
			public string type { get; set; }
			public double version { get; set; }
			public int width { get; set; }
		}
	}
}
