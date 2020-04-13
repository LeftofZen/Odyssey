using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGui;
using Noise;
using System;
using System.Collections.Generic;

namespace MonogameTest1
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game2 : Game
	{
		readonly GraphicsDeviceManager graphics;
		SpriteBatch sb;
		readonly Dictionary<string, Texture2D> texLookup = new Dictionary<string, Texture2D>();
		readonly Dictionary<string, SpriteFont> fontLookup = new Dictionary<string, SpriteFont>();
		const int mapWidth = 32;
		const int mapHeight = 32;
		int[,] map = new int[mapWidth, mapHeight];
		const int tileSize = 32;
		Dictionary<string, Map> mapLookup = new Dictionary<string, Map>();
		NoiseParams noiseSettings { get; set; } = new NoiseParams();
		NoiseParams previousNoiseSettings { get; set; } = new NoiseParams();
		bool isFullScreen = false;
		bool animate = true;
		Vector2 animationOffset = Vector2.Zero;
		bool colourMapEnabled = true;
		float animationSpeed = 1f;
		float animationDirection = 0f;

		MouseState previousMouseState;

		// colour map imgui settings
		System.Numerics.Vector3 v1 = new System.Numerics.Vector3(0f);
		System.Numerics.Vector3 v2 = new System.Numerics.Vector3(0.2f);
		System.Numerics.Vector3 v3 = new System.Numerics.Vector3(0.4f);
		System.Numerics.Vector3 v4 = new System.Numerics.Vector3(0.6f);
		System.Numerics.Vector3 v5 = new System.Numerics.Vector3(0.8f);
		System.Numerics.Vector3 v6 = new System.Numerics.Vector3(1f);

		float p1 = 0.2f;
		float p2 = 0.4f;
		float p3 = 0.6f;
		float p4 = 0.8f;

		public ImGUIRenderer GuiRenderer; //This is the ImGuiRenderer

		public Game2()
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

			//map
			var rnd = new Random();
			for (int y = 0; y < mapHeight; y++)
			{
				for (int x = 0; x < mapWidth; x++)
				{
					map[x, y] = rnd.Next(484);
				}
			}

			previousMouseState = Mouse.GetState();

			base.Initialize();

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
			var texNames = new List<string> { "terrain" };
			foreach (var v in texNames)
			{
				texLookup.Add(v, Content.Load<Texture2D>("textures\\" + v));
			}

			var pixel = new Texture2D(GraphicsDevice, 1, 1);
			pixel.SetData(new Color[] { Color.White });
			texLookup.Add(nameof(pixel), pixel);

			var noiseTex = CreateNoiseTexture(null);
			texLookup.Add(nameof(noiseTex), noiseTex);

			var fontNames = new List<string> { "Calibri" };
			foreach (var v in fontNames)
			{
				fontLookup.Add(v, Content.Load<SpriteFont>("fonts\\" + v));
			}
		}

		private Texture2D CreateNoiseTexture(GameTime gameTime)
		{
			var noiseTex = new Texture2D(GraphicsDevice, noiseSettings.NoiseSize, noiseSettings.NoiseSize);
			var noise = new OpenSimplexNoise(0);
			var colArray = new Color[noiseSettings.NoiseSize * noiseSettings.NoiseSize];
			if (animate && gameTime != null)
			{
				var dir = new Vector2((float)Math.Cos(animationDirection), (float)Math.Sin(animationDirection));
				dir.Normalize();
				animationOffset +=  dir * new Vector2(0.01f * animationSpeed);
			}

			for (var i = 0; i < colArray.Length; i++)
			{
				var amplitude = noiseSettings.InitialAmplitude;
				var frequency = noiseSettings.InitialFrequency;
				var totalAmplitude = 0f;
				var total = 0f;

				for (var o = 1; o < noiseSettings.Octaves + 1; o++)
				{
					var noisev = (float)noise.Evaluate(
						(i % noiseSettings.NoiseSize + noiseSettings.Offset.X + animationOffset.X) * frequency,
						(i / noiseSettings.NoiseSize + noiseSettings.Offset.Y + animationOffset.Y) * frequency);

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

				// colour gradient
				var color = new Color(total, total, total);

				if (colourMapEnabled)
				{
					var c1 = new Color(v1.X, v1.Y, v1.Z);
					var c2 = new Color(v2.X, v2.Y, v2.Z);
					var c3 = new Color(v3.X, v3.Y, v3.Z);
					var c4 = new Color(v4.X, v4.Y, v4.Z);
					var c5 = new Color(v5.X, v5.Y, v5.Z);
					var c6 = new Color(v6.X, v6.Y, v6.Z);

					if (total <= p1)      { color = Color.Lerp(c1, c2, MathHelper.SmoothStep(0, 1, (total - 0f) / (p1 - 0f))); }
					else if (total <= p2) { color = Color.Lerp(c2, c3, MathHelper.SmoothStep(0, 1, (total - p1) / (p2 - p1))); }
					else if (total <= p3) { color = Color.Lerp(c3, c4, MathHelper.SmoothStep(0, 1, (total - p2) / (p3 - p2))); }
					else if (total <= p4) { color = Color.Lerp(c4, c5, MathHelper.SmoothStep(0, 1, (total - p3) / (p4 - p3))); }
					else                  { color = Color.Lerp(c5, c6, MathHelper.SmoothStep(0, 1, (total - p4) / (1f - p4))); }
				}

				// greyscale
				colArray[i] = color;
			}
			noiseTex.SetData(colArray);
			return noiseTex;
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
			if (new Rectangle(20, 20, 20 + 1024, 20 + 1024).Contains(mouse.Position))
			{
				if (mouse.LeftButton == ButtonState.Pressed)
				{
					noiseSettings.Offset += (previousMouseState.Position - mouse.Position).ToVector2() / 10f;
				}
			}
			previousMouseState = mouse;

			// TODO: Add your update logic here
			//if (!noiseSettings.IsEqualTo(previousNoiseSettings))
			{
				var noiseTex = CreateNoiseTexture(gameTime);
				texLookup[nameof(noiseTex)] = noiseTex;
				previousNoiseSettings.Set(noiseSettings);
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);
			//int drawCount = 0;
			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);

			//sb.Draw(texLookup["pixel"], new Vector2(200, 200), Color.White);
			sb.Draw(texLookup["noiseTex"], new Rectangle(20, 20, 20 + 1024, 20 + 1024), null, Color.White);
			//DrawMap(sb, mapLookup["map1"]);
			//DrawDebugString(sb, fontLookup["Calibri"], $"DrawCount={drawCount}", new Vector2(8, 8));
			//sb.Draw(texLookup["terrain"], Vector2.Zero, Color.White);
			sb.End();

			base.Draw(gameTime);

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

			if (ImGui.CollapsingHeader("Animation Settings", ImGuiTreeNodeFlags.DefaultOpen))
			{
				_ = ImGui.Checkbox("Animate", ref animate);
				_ = ImGui.SliderFloat("Speed", ref animationSpeed, 0f, 100f);
				_ = ImGui.SliderFloat("Direction (radians)", ref animationDirection, 0f, (float)Math.PI * 2f);
			}

			if (ImGui.CollapsingHeader("Noise Settings", ImGuiTreeNodeFlags.DefaultOpen))
			{
				_ = ImGui.SliderInt("Pixels", ref noiseSettings.NoiseSize, 1, 512);
				_ = ImGui.SliderInt("Octaves", ref noiseSettings.Octaves, 1, 8);
				_ = ImGui.SliderFloat("Initial Amplitude", ref noiseSettings.InitialAmplitude, 0f, 1f);
				_ = ImGui.SliderFloat("Initial Frequency", ref noiseSettings.InitialFrequency, 0f, 1f);
				_ = ImGui.SliderFloat("Lacunarity", ref noiseSettings.Lacunarity, 0f, 4f);
				_ = ImGui.SliderFloat("Persistence", ref noiseSettings.Persistence, 0f, 4f);
				_ = ImGui.SliderFloat("OffsetX", ref noiseSettings.Offset.Y, -10f, 10f);
				_ = ImGui.SliderFloat("OffsetY", ref noiseSettings.Offset.X, -10f, 10f);
			}

			if (ImGui.CollapsingHeader("Colour Map", ImGuiTreeNodeFlags.DefaultOpen))
			{
				_ = ImGui.Checkbox("Enable", ref colourMapEnabled);
				_ = ImGui.ColorEdit3("Colour1", ref v1);
				_ = ImGui.ColorEdit3("Colour2", ref v2);
				_ = ImGui.ColorEdit3("Colour3", ref v3);
				_ = ImGui.ColorEdit3("Colour4", ref v4);
				_ = ImGui.ColorEdit3("Colour5", ref v5);
				_ = ImGui.ColorEdit3("Colour6", ref v6);

				_ = ImGui.SliderFloat("Position1", ref p1, 0f, p2);
				_ = ImGui.SliderFloat("Position2", ref p2, p1, p3);
				_ = ImGui.SliderFloat("Position3", ref p3, p2, p4);
				_ = ImGui.SliderFloat("Position4", ref p4, p3, 1f);
			}

			if (ImGui.CollapsingHeader("Debug Data", ImGuiTreeNodeFlags.DefaultOpen))
			{
				ImGui.Text($"Animation Offset {animationOffset}");
			}

				GuiRenderer.EndLayout();
		}

		//public static void DrawDebugString(SpriteBatch sb, SpriteFont sf, string str, Vector2 position, float scale = 1f)
		//{
		//	sb.DrawString(sf, str, position + Vector2.One, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		//	sb.DrawString(sf, str, position, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		//}
	}
}
