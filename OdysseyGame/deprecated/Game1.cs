using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;

namespace MonogameTest1
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		private readonly GraphicsDeviceManager graphics;
		private SpriteBatch sb;
		private readonly Dictionary<string, Texture2D> texLookup = new Dictionary<string, Texture2D>();
		private readonly Dictionary<string, SpriteFont> fontLookup = new Dictionary<string, SpriteFont>();
		private const int mapWidth = 32;
		private const int mapHeight = 32;
		private int[,] map = new int[mapWidth, mapHeight];
		private const int tileSize = 32;
		private Dictionary<string, Tiled.Map> mapLookup = new Dictionary<string, Tiled.Map>();

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1920,
				PreferredBackBufferHeight = 1080
			};

			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			IsMouseVisible = true;

			//map
			var rnd = new Random();
			for (var y = 0; y < mapHeight; y++)
			{
				for (var x = 0; x < mapWidth; x++)
				{
					map[x, y] = rnd.Next(484);
				}
			}

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			sb = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			var texNames = new List<string> { "terrain" };
			foreach (var v in texNames)
			{
				texLookup.Add(v, Content.Load<Texture2D>("textures\\" + v));
			}

			var fontNames = new List<string> { "Calibri" };
			foreach (var v in fontNames)
			{
				fontLookup.Add(v, Content.Load<SpriteFont>("fonts\\" + v));
			}

			mapLookup.Add("map1", JObject.Parse(File.ReadAllText(@"C:\Users\bigba\OneDrive\Documents\Tiled\map1.json")).ToObject<Tiled.Map>());

			//var mapNames = new List<string> { "map1" };
			//foreach (var v in mapNames)
			//{
			//	var file = File.ReadAllText(Content.RootDirectory + "\\maps\\" + v + ".json");
			//	mapLookup.Add(v, JObject.Parse(file).ToObject<Map>());
			//}
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// game-specific content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
			}

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			var drawCount = 0;
			sb.Begin();

			// TODO: Add your drawing code here
			//for (int y = 0; y < map.GetLength(1); y++)
			//{
			//	for (int x = 0; x < map.GetLength(0); x++)
			//	{
			//		// don't render past screen edge
			//		var tileDrawPos = new Vector2(x * tileSize, y * tileSize);
			//		if (tileDrawPos.X > GraphicsDevice.Viewport.Width || tileDrawPos.Y > GraphicsDevice.Viewport.Height)
			//		{
			//			break;
			//		}

			//		var tileId = map[x, y];
			//		var tilesPerRow = texLookup["terrain"].Width / tileSize;

			//		var tileX = tileId % tilesPerRow * tileSize;
			//		var tileY = tileId / tilesPerRow * tileSize;

			//		var lookupRect = new Rectangle(tileX, tileY, tileSize, tileSize);
			//		//sb.Draw(texLookup["terrain"], tileDrawPos, lookupRect, Color.White);

			//		// tileId
			//		DrawDebugString(sb, fontLookup["Calibri"], $"{tileId}", tileDrawPos + new Vector2(1), 1f);

			//		drawCount++;
			//	}
			//}

			DrawMap(sb, mapLookup["map1"]);
			DrawDebugString(sb, fontLookup["Calibri"], $"DrawCount={drawCount}", new Vector2(8, 8));
			//sb.Draw(texLookup["terrain"], Vector2.Zero, Color.White);

			sb.End();
			base.Draw(gameTime);
		}

		public void DrawMap(SpriteBatch sb, Tiled.Map m)
		{
			foreach (var l in m.layers)
			{
				for (var y = 0; y < l.height; y++)
				{
					for (var x = 0; x < l.width; x++)
					{
						// don't render past screen edge
						var tileDrawPos = new Vector2(x * m.tilewidth, y * m.tileheight);
						if (tileDrawPos.X > GraphicsDevice.Viewport.Width || tileDrawPos.Y > GraphicsDevice.Viewport.Height)
						{
							break;
						}

						var tileId = l.data[x + y * l.width] - m.tilesets.First().firstgid;
						var tilesPerRow = texLookup["terrain"].Width / tileSize;

						var tileX = tileId % tilesPerRow * tileSize;
						var tileY = tileId / tilesPerRow * tileSize;

						var lookupRect = new Rectangle(tileX, tileY, tileSize, tileSize);
						sb.Draw(texLookup["terrain"], tileDrawPos, lookupRect, Color.White);

						DrawDebugString(sb, fontLookup["Calibri"], $"{tileId}", tileDrawPos + new Vector2(1), 1f);
					}
				}
			}
		}

		public static void DrawDebugString(SpriteBatch sb, SpriteFont sf, string str, Vector2 position, float scale = 1f)
		{
			sb.DrawString(sf, str, position + Vector2.One, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			sb.DrawString(sf, str, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}
}
