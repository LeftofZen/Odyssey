using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using Odyssey.Entities;
using Odyssey.Logging;
using Odyssey.Network;
using Odyssey.Networking;
using Odyssey.Networking.Messages;
using Odyssey.Noise;
using Odyssey.World;
using Serilog;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Odyssey.Server
{
	public class ServerProcess : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch sb;
		private OdysseyServer server;
		private InMemorySink logsink;
		private bool renderLog;

		#region Game State

		private readonly Player player = new();
		private Map map;
		private const int initialMapWidth = 32;
		private const int initialMapHeight = 32;
		private const int tileSize = 32;
		private NoiseParams NoiseSettings { get; } = new NoiseParams();
		private NoiseParams PreviousNoiseSettings { get; set; } = new NoiseParams();

		#endregion

		public ServerProcess()
		{
			logsink = new InMemorySink();
			Log.Logger = new LoggerConfiguration()
				//.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // https://github.com/serilog/serilog/wiki/Formatting-Output
				.WriteTo.Sink(logsink)
				.MinimumLevel.Debug()
				.CreateLogger();

			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.Title = "Odyssey (server)";

			server = new OdysseyServer();

		}
		protected override void OnExiting(object sender, EventArgs args)
		{
			_ = server.Stop();
			base.OnExiting(sender, args);
		}

		protected override void Initialize()
		{
			// world
			NoiseSettings.NoiseSize = 32;
			map = new Map(initialMapWidth, initialMapHeight, NoiseHelpers.CreateNoise2D(NoiseSettings))
			{
				TileSize = 32
			};

			// character
			//player.Position = new Vector2(map.Width * tileSize / 2, map.Height * tileSize / 2);
			player.Position = new Vector2(100, 100);
			player.MoveSpeed = 4f;
			player.Name = "Left of Zen";

			_ = server.Start();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			sb = new SpriteBatch(GraphicsDevice); // TODO: use this.Content to load your game content here
			GameServices.LoadContent(Content, GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			var kb = KeyboardExtended.GetState();
			if (kb.WasKeyJustDown(Keys.OemTilde))
			{
				renderLog = !renderLog;
			}

			server.Update(gameTime);

			NetworkReceive(gameTime);
			NetworkSend();

			base.Update(gameTime);
		}

		private void NetworkSend()
		{
			server.SendMessageToAllClients(NetworkMessageType.WorldUpdate, new WorldUpdate() { Map = map });

			// foreach player
			server.SendMessageToClient(player.Name, NetworkMessageType.PlayerUpdate, new PlayerUpdate() { Position = player.Position });
		}

		private void NetworkReceive(GameTime gameTime)
		{
			// process server queue
			while (server.MessageQueue.TryDequeue(out var msg))
			{
				Log.Information("got message in game loop from server queue", msg);
				switch (msg.Type)
				{
					case NetworkMessageType.NetworkInput:
						var networkMsg = (Messages)msg;
						Log.Information("[NetworkInput Message] {inputTime}", networkMsg.InputTimeUnixMilliseconds);
						// input handling above, everything else below
						player.Update(networkMsg, gameTime);
						break;
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.LightGray);

			sb.Begin();

			var scale = 1f / 4f;
			MapRenderer.Draw(sb, map, (int)(tileSize * scale));
			EntityRenderer.Draw(sb, player, scale);

			sb.End();

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			if (renderLog)
			{
				InMemorySinkRenderer.Draw(logsink, sb, 10, 10);
			}

			sb.End();

			base.Draw(gameTime);
		}
	}

	public static class MapRenderer
	{
		public static void Draw(SpriteBatch sb, Map map, int tileSize = 32)
		{
			for (var y = 0; y < map.Height; y++)
			{
				for (var x = 0; x < map.Width; x++)
				{
					sb.FillRectangle(new RectangleF(x * tileSize, y * tileSize, tileSize, tileSize), map.At(x, y).Colour);
				}
			}
		}
	}

	public static class EntityRenderer
	{
		public static void Draw(SpriteBatch sb, Player player, float scale)
			=> sb.FillRectangle(new RectangleF(player.GetPosition().X * scale, player.GetPosition().X * scale, player.GetSize().X * scale, player.GetSize().Y * scale), Color.Chocolate);
	}
}