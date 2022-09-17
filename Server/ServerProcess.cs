using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			server = new OdysseyServer();

			logsink = new InMemorySink();
			Log.Logger = new LoggerConfiguration()
				//.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // https://github.com/serilog/serilog/wiki/Formatting-Output
				.WriteTo.Sink(logsink)
				.MinimumLevel.Debug()
				.CreateLogger();
		}
		protected override void OnExiting(object sender, EventArgs args)
		{
			_ = server.Stop();
			base.OnExiting(sender, args);
		}

		protected override void Initialize()
		{
			// world
			map = new Map(initialMapWidth, initialMapHeight, NoiseHelpers.CreateNoise2D(NoiseSettings))
			{
				TileSize = 32
			};

			// character
			player.Position = new Vector2(map.Width * tileSize / 2, map.Height * tileSize / 2);
			player.MoveSpeed = 4f;
			player.Name = "Left of Zen";

			server.Start();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			sb = new SpriteBatch(GraphicsDevice); // TODO: use this.Content to load your game content here
			GameServices.LoadContent(Content, GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			server.ReadMessages();
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
						var networkMsg = (NetworkInput)msg;
						Log.Information("[NetworkInput Message] {inputTime}", networkMsg.InputTimeUnixMilliseconds);
						// input handling above, everything else below
						player.Update(networkMsg, gameTime);
						break;
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.DarkGray);

			// logs
			var logStartY = 100;
			foreach (var log in logsink.Events.TakeLast(30))
			{
				Odyssey.Render.String.DrawDebugStringLeftAligned(sb, GameServices.Fonts["Calibri"], log.message, new Vector2(100, logStartY), Color.Red, 1);
				logStartY += 20;
			}

			base.Draw(gameTime);
		}
	}
}