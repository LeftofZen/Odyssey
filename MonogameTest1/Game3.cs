using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.ImGui;
using Newtonsoft.Json.Schema;
using Noise;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonogameTest1
{
	public interface IEntity
	{
		Vector2 GetPosition();
		Vector2 GetSize();

		float GetMovementSpeed();

		string GetName();

		void SetPosition(Vector2 pos);
	}

	// I know XNA/Monogame has some kind of GameServices like this with components, need to look that up
	// to implement this pattern correctly and avoid rewriting code
	public static class GameServices
	{
		public static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>(); // TextureCollection??
		public static readonly Dictionary<string, SpriteFont> Fonts = new Dictionary<string, SpriteFont>(); //
		public static readonly Dictionary<string, SoundEffect> SoundEffects = new Dictionary<string, SoundEffect>(); // SoundBank??
		public static readonly Dictionary<string, Song> Songs = new Dictionary<string, Song>(); // SongCollection??
	}

	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game3 : Game
	{
		readonly GraphicsDeviceManager graphics;
		SpriteBatch sb;
		const int initialMapWidth = 32;
		const int initialMapHeight = 32;
		const int tileSize = 32;
		Dictionary<string, Map> mapLookup = new Dictionary<string, Map>();
		NoiseParams noiseSettings { get; set; } = new NoiseParams();
		NoiseParams previousNoiseSettings { get; set; } = new NoiseParams();
		bool isFullScreen = false;

		MouseState previousMouseState;
		float volume = 0.5f;
		Vector2 pos2 = Vector2.Zero;

		public ImGUIRenderer GuiRenderer; //This is the ImGuiRenderer

		Player player1 = new Player();
		Camera camera;
		Map map;
		List<Animal> animals;
		Random rand = new Random();

		public Game3()
		{
			graphics = new GraphicsDeviceManager(this);
			graphics.PreferredBackBufferWidth = 1980;
			graphics.PreferredBackBufferHeight = 1080;

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
			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			// world
			map = new Map(initialMapWidth, initialMapHeight, CreateNoise2D(noiseSettings));
			map.TileSize = 32;

			// character
			player1.Position = new Vector2(map.Width * tileSize / 2, map.Height * tileSize / 2);
			player1.MoveSpeed = 4f;
			player1.Name = "Left of Zen";

			// entities
			animals = new List<Animal>();
			// player pet doggy
			var pet = new Animal { Position = player1.Position, MoveSpeed = 3f, Name = "Fluffy", AnimalType = AnimalType.Dog };
			pet.Behaviours.Add(new FollowBehaviour { Target = player1 });
			animals.Add(pet);

			//var animalCount = 20;
			//for (var i = 0; i < animalCount; i++)
			//{
			//	var rnd = rand.Next(1, 3);
			//	var type = (AnimalType)rnd;
			//	animals.Add(new Animal
			//	{
			//		Position = player1.Position + new Vector2(rand.Next(-1024, 1024), rand.Next(-1024, 1024)),
			//		MoveSpeed = 4f,
			//		Name = $"{type}-{i}",
			//		AnimalType = type
			//	});
			//}

			// camera
			camera = new Camera(GraphicsDevice.Viewport);

			// input
			previousMouseState = Mouse.GetState();

			base.Initialize();

			// ui
			GuiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();
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
			var texNames = new List<string> { "terrain", "char", "ui", "animals" };
			foreach (var v in texNames)
			{
				GameServices.Textures.Add(v, Content.Load<Texture2D>("textures\\" + v));
			}

			var pixel = new Texture2D(GraphicsDevice, 1, 1);
			pixel.SetData(new Color[] { Color.White });
			GameServices.Textures.Add(nameof(pixel), pixel);

			var fontNames = new List<string> { "Calibri" };
			foreach (var v in fontNames)
			{
				GameServices.Fonts.Add(v, Content.Load<SpriteFont>("fonts\\" + v));
			}

			var songNames = new List<string> { "farm_music" };
			foreach (var v in songNames)
			{
				GameServices.Songs.Add(v, Content.Load<Song>("songs\\" + v));
			}

			var sfxNames = new List<string> { "dogbark", "dog2", "ponywhinny", "rooster" };
			foreach (var v in sfxNames)
			{
				GameServices.SoundEffects.Add(v, Content.Load<SoundEffect>("soundeffects\\" + v));
			}

			//MediaPlayer.Play(GameServices.Songs["farm_music"]);
		}

		private static float[,] CreateNoise2D(NoiseParams noiseSettings)
		{
			var noise = new OpenSimplexNoise(noiseSettings.Seed);
			var data = new float[noiseSettings.NoiseSize, noiseSettings.NoiseSize];
			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					var amplitude = noiseSettings.InitialAmplitude;
					var frequency = noiseSettings.InitialFrequency;
					var totalAmplitude = 0f;
					var total = 0f;
					var xEval = x + noiseSettings.Offset.X;
					var yEval = y + noiseSettings.Offset.Y;

					for (var o = 1; o < noiseSettings.Octaves + 1; o++)
					{
						var noisev = (float)noise.Evaluate(xEval * frequency, yEval * frequency);

						// [[-1, 1] -> [0, 1]
						noisev = (noisev + 1) / 2;
						noisev *= amplitude;
						total += noisev;

						totalAmplitude += amplitude;
						amplitude *= noiseSettings.Persistence;
						frequency *= noiseSettings.Lacunarity;
					}

					// normalise
					total /= totalAmplitude;

					data[x, y] = total;
				}
			}

			var dd = NormaliseNoise2D(data);

			if (noiseSettings.UseKernel)
			{
				var identityKernel = new float[,] { { 1f } };
				var smoothingKernel = new float[,]
				{
					{ 1f, 1f, 1f },
					{ 1f, 1f, 1f },
					{ 1f, 1f, 1f },
				};

				var sharpenKernel = new float[,]
				{
					{ 0f, -1f, 0f },
					{ -1f, 5f, -1f },
					{ 0f, -1f, 0f },
				};

				var outlineKernel = new float[,]
				{
					{ -1f, -1f, -1f },
					{ -1f, 8f, -1f },
					{ -1f, -1f, -1f },
				};

				var topSobel = new float[,]
				{
					{ 1f, 2f, 1f },
					{ 0f, 0f, 0f },
					{ -1f, -2f, -1f },
				};
				dd = ApplyKernel(dd, topSobel);
			}


			var erosion = new Erosion();
			var dmap = To1D(dd);

			//erosion.Erode(dmap, 256, 10, false);
			dd = To2D(dmap);

			var res = AddBorder(dd);
			return res;
		}

		private static float[] To1D(float[,] data)
		{
			float[] result = new float[data.GetLength(0) * data.GetLength(1)];
			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					if (float.IsNaN(data[x, y]))
					{
						Debugger.Break();
					}
					result[y * data.GetLength(0) + x] = data[x, y];
				}
			}
			return result;
		}
		private static float[,] To2D(float[] data)
		{
			var size = (int)Math.Sqrt(data.Length);
			float[,] result = new float[size, size];

			for (int i = 0; i < data.Length; i++)
			{
				result[i % size, i / size] = data[i];
			}
			return result;
		}


		private static float[,] NormaliseNoise2D(float[,] data)
		{
			float min = float.MaxValue;
			float max = float.MinValue;
			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					min = Math.Min(min, data[x, y]);
					max = Math.Max(max, data[x, y]);
				}
			}

			var range = max - min;
			var xx = 1f / range;

			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					data[x, y] = (data[x, y] - min) * xx;
				}
			}
			return data;
		}

		private static float[,] AddBorder(float[,] data, float borderValue = 0f)
		{
			// top, bottom
			for (var x = 0; x < data.GetLength(0); x++)
			{
				data[x, 0] = borderValue;
				data[x, data.GetLength(1) - 1] = borderValue;
			}

			// left, right
			for (var y = 0; y < data.GetLength(1); y++)
			{
				data[0, y] = borderValue;
				data[data.GetLength(0) - 1, y] = borderValue;
			}

			return data;
		}

		private static float[,] ApplyKernel(float[,] data, float[,] kernel)
		{
			float[,] tmp = new float[data.GetLength(0), data.GetLength(1)];
			var kernelSize = kernel.GetLength(0) * kernel.GetLength(1);

			var halfsizeX = kernel.GetLength(0) / 2;
			var halfsizeY = kernel.GetLength(1) / 2;

			var kernelSpotsUsed = 0;
			for (var ky = 0; ky < kernel.GetLength(1); ky++)
			{
				for (var kx = 0; kx < kernel.GetLength(0); kx++)
				{
					if (kernel[kx, ky] != 0f)
					{
						kernelSpotsUsed++;
					}
				}
			}

			for (var y = halfsizeY; y < data.GetLength(1) - halfsizeY; y++)
			{
				for (var x = halfsizeX; x < data.GetLength(0) - halfsizeX; x++)
				{
					// apply kernel

					// even size kernel

					// odd size kernel
					var tmpres = 0f;

					//kernel
					for (var ky = 0; ky < kernel.GetLength(1); ky++)
					{
						for (var kx = 0; kx < kernel.GetLength(0); kx++)
						{
							tmpres += data[x + kx - halfsizeX, y + ky - halfsizeY] * kernel[kx, ky];
						}
					}

					tmp[x, y] = tmpres;
				}
			}

			// can remove this normalise if we normalise tmpres above (requires getting min/max of kernel
			return NormaliseNoise2D(tmp);
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

			var mouse = Mouse.GetState();
			if (new Rectangle(0, 0, 1024, 1024).Contains(mouse.Position))
			{
				if (mouse.LeftButton == ButtonState.Pressed)
				{
					noiseSettings.Offset += (previousMouseState.Position - mouse.Position).ToVector2() / 10f;
				}
			}
			previousMouseState = mouse;

			// input handling above, everything else below
			player1.Update(gameTime);

			foreach (var a in animals)
			{
				//if (a.AnimalType == AnimalType.Dog)
				//{
				//	a.TargetPosition = player1.Position - new Vector2(0, 24);
				//}
				//else
				//{
				//	if (a.AtTarget && (int)(gameTime.TotalGameTime.TotalMilliseconds) % (3000 + 100 * a.Name.GetHashCode() % 50) == 0)
				//	{
				//		a.TargetPosition = player1.Position - new Vector2(0, 24) + new Vector2(rand.Next(-1024, 1024), rand.Next(-1024, 1024));
				//	}
				//}

				a.Update(gameTime);
			}

			MediaPlayer.Volume = volume;

			// TODO: Add your update logic here
			if (!noiseSettings.IsEqualTo(previousNoiseSettings))
			{
				map.SetData(CreateNoise2D(noiseSettings));
				previousNoiseSettings.Set(noiseSettings);
			}

			camera.Follow(player1.Position);
			camera.UpdateCamera(GraphicsDevice.Viewport);

			base.Update(gameTime);
		}

		public Vector2 ScreenToWorldSpace(Vector2 point, Matrix transform)
		{
			Matrix invertedMatrix = Matrix.Invert(transform);
			return Vector2.Transform(point, invertedMatrix);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.SteelBlue);

			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.Transform);

			// draw world
			map.Draw(sb, gameTime, camera);

			// draw highlighted tile
			var mousePos = Mouse.GetState().Position.ToVector2();
			mousePos = ScreenToWorldSpace(mousePos, camera.Transform);
			DrawTileAlignedBox(mousePos, tileSize);

			// draw animals
			foreach (var a in animals)
			{
				a.Draw(sb, gameTime);
				DrawTileAlignedBox(a.Position, tileSize);
				DrawBoundingBox(a);
				if (camera.Zoom >= 1)
					DrawDebugString(sb, GameServices.Fonts["Calibri"], a.Name, a.Position - new Vector2(0, a.Size.Y / 2), Color.White);
			}

			// draw player
			player1.Draw(sb, gameTime);
			DrawTileAlignedBox(player1.Position, tileSize);
			DrawBoundingBox(player1);
			if (camera.Zoom >= 1)
				DrawDebugString(sb, GameServices.Fonts["Calibri"], player1.Name, player1.Position - new Vector2(0, player1.Size.Y / 2), Color.White);

			//DrawMap(sb, mapLookup["map1"]);
			//DrawDebugString(sb, fontLookup["Calibri"], $"DrawCount={drawCount}", new Vector2(8, 8));
			//sb.Draw(texLookup["terrain"], Vector2.Zero, Color.White);
			sb.End();

			// non-camera things, ie ui things
			sb.Begin();
			if (camera.Zoom < 1)
			{
				foreach (var a in animals)
				{
					DrawDebugString(sb, GameServices.Fonts["Calibri"], a.Name, Vector2.Transform(a.Position - new Vector2(0, a.Size.Y / 2), camera.Transform), Color.White);
				}

				DrawDebugString(sb, GameServices.Fonts["Calibri"], player1.Name, Vector2.Transform(player1.Position - new Vector2(0, player1.Size.Y / 2), camera.Transform), Color.White);
			}
			sb.End();

			base.Draw(gameTime);

			DrawImGui(gameTime);
		}

		void DrawTileAlignedBox(Vector2 position, int alignment)
		{
			position.X -= position.X % alignment;
			position.Y -= position.Y % alignment;
			sb.Draw(
				GameServices.Textures["ui"],
				position,
				new Rectangle(0, 0, tileSize, tileSize),
				Color.Red);
		}

		void DrawBoundingBox(IEntity entity)
		{
			var pos = (entity.GetPosition() - (entity.GetSize() / 2)).ToPoint();
			sb.Draw(
				GameServices.Textures["ui"],
				new Rectangle(pos.X, pos.Y, (int)entity.GetSize().X, (int)entity.GetSize().Y),
				new Rectangle(0, 0, 32, 32),
				Color.Blue);
		}

		public void DrawDebugString(SpriteBatch sb, SpriteFont sf, string str, Vector2 pos, Color color, float scale = 1f)
		{
			Vector2 size = sf.MeasureString(str);
			sb.DrawString(sf, str, pos - size / 2 + Vector2.One, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			sb.DrawString(sf, str, pos - size / 2, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		void DrawImGui(GameTime gameTime)
		{
			// ImGUI
			GuiRenderer.BeginLayout(gameTime);
			if (ImGui.CollapsingHeader("Program Settings", ImGuiTreeNodeFlags.DefaultOpen))
			{
				//_ = ImGui.Checkbox("Fullscreen", ref isFullScreen);
				//if (isFullScreen != graphics.IsFullScreen)
				//{
				//	graphics.ToggleFullScreen();
				//}
			}

			if (ImGui.CollapsingHeader("Map", ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.Text($"Width={map.Width}");
				ImGui.Text($"Height={map.Height}");
				_ = ImGui.Checkbox($"DrawNoiseOnly={map.DrawNoiseOnly}", ref map.DrawNoiseOnly);
				//ImGui.Checkbox($"UseColourMap={map.UseColourMap}", ref map.UseColourMap);
			}

			if (ImGui.CollapsingHeader("Player", ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.Text($"Name={player1.Name}");
				ImGui.Text($"Position={player1.Position}");
				ImGui.Text($"ScreenPosition={pos2}");
			}

			if (ImGui.CollapsingHeader("Animals", ImGuiTreeNodeFlags.DefaultOpen))
			{
				foreach (var v in animals)
				{
					ImGui.Text($"Name={v.Name}");
					ImGui.Text($"Position={v.Position}");
				}
			}

			if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.Text($"Position={camera.Position}");
				ImGui.Text($"Zoom={camera.Zoom}");
				ImGui.Text($"VisibleArea={camera.VisibleArea}");
				ImGui.Text($"Bounds={camera.Bounds}");
			}

			if (ImGui.CollapsingHeader("Music", ImGuiTreeNodeFlags.DefaultOpen))
			{
				if (ImGui.Button("Play"))
				{
					MediaPlayer.IsRepeating = true;
					MediaPlayer.Play(GameServices.Songs["farm_music"]);
				}
				if (ImGui.Button("Stop"))
				{
					MediaPlayer.Stop();
				}

				_ = ImGui.SliderFloat("Volume", ref volume, 0f, 1f);
				int currentPos = (int)MediaPlayer.PlayPosition.TotalSeconds;
				_ = ImGui.SliderInt("Position", ref currentPos, 0, (int)GameServices.Songs["farm_music"].Duration.TotalSeconds);
			}

			if (ImGui.CollapsingHeader("Noise Settings", ImGuiTreeNodeFlags.DefaultOpen))
			{
				_ = ImGui.SliderInt("Pixels", ref noiseSettings.NoiseSize, 1, 512);
				_ = ImGui.SliderInt("Octaves", ref noiseSettings.Octaves, 1, 8);
				_ = ImGui.SliderFloat("Initial Amplitude", ref noiseSettings.InitialAmplitude, 0f, 1f);
				_ = ImGui.SliderFloat("Initial Frequency", ref noiseSettings.InitialFrequency, 0f, 1f);
				_ = ImGui.SliderFloat("Lacunarity", ref noiseSettings.Lacunarity, 0f, 4f);
				_ = ImGui.SliderFloat("Persistence", ref noiseSettings.Persistence, 0f, 4f);
				_ = ImGui.SliderFloat("OffsetX", ref noiseSettings.Offset.Y, -100f, 100f);
				_ = ImGui.SliderFloat("OffsetY", ref noiseSettings.Offset.X, -100f, 100f);
				_ = ImGui.Checkbox($"UseKernel={noiseSettings.UseKernel}", ref noiseSettings.UseKernel);
			}

			if (ImGui.CollapsingHeader("Debug Data", ImGuiTreeNodeFlags.DefaultOpen))
			{
				//ImGui.Text($"Animation Offset={animationOffset}");
				ImGui.Text($"GameTime.TotalGameTime.TotalMilliseconds={gameTime.TotalGameTime.TotalMilliseconds}");
			}

			GuiRenderer.EndLayout();
		}
	}
}
