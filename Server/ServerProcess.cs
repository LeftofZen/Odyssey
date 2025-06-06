﻿using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;
using Odyssey.ECS;
using Odyssey.Entities;
using Odyssey.Logging;
using Odyssey.Messaging;
using Odyssey.Messaging.Messages;
using Odyssey.Noise;
using Odyssey.Render;
using Odyssey.World;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Odyssey.Server
{
	public class ServerProcess : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch sb;
		private OdysseyServer server;
		private ILogger Logger;
		private InMemorySink logsink;
		private bool renderLog = true;

		#region Game State

		private GameState gameState;

		private const int initialMapWidth = 32;
		private const int initialMapHeight = 32;
		private const int tileSize = 32;
		private NoiseParams NoiseSettings { get; } = new NoiseParams();
		private NoiseParams PreviousNoiseSettings { get; set; } = new NoiseParams();

		#endregion

		private ImGuiRenderer GuiRenderer;

		public Vector2 clientMousePos = Vector2.Zero;

		public ServerProcess()
		{
			ClearLogs();

			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1080,
				PreferredBackBufferHeight = 768
			};

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.Title = "Odyssey (server)";

			gameState = new()
			{
				Entities = []
			};

			server = new OdysseyServer(Logger);

		}

		private void ClearLogs()
		{
			logsink = new InMemorySink();
			Logger = new LoggerConfiguration()
				//.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // https://github.com/serilog/serilog/wiki/Formatting-Output
				.WriteTo.Sink(logsink)
				.Enrich.WithExceptionDetails()
				.MinimumLevel.Debug()
				.CreateLogger();

			Log.Logger = Logger;
		}

		protected override void OnExiting(object sender, ExitingEventArgs args)
		{
			_ = server.Stop();
			base.OnExiting(sender, args);
		}

		protected override void Initialize()
		{
			// imgui
			GuiRenderer = new ImGuiRenderer(this);
			GuiRenderer.RebuildFontAtlas();

			// world
			NoiseSettings.NoiseSize = 32;
			gameState.Map = new Map(initialMapWidth, initialMapHeight, NoiseHelpers.CreateNoise2D(NoiseSettings))
			{
				TileSize = 32
			};

			// character
			//player.Position = new Vector2(map.Width * tileSize / 2, map.Height * tileSize / 2);
			//var player = new Player
			//{
			//	Id = Guid.NewGuid(),
			//	Position = new Vector2(100, 100),
			//	MoveSpeed = 4f,
			//};
			//gameState.Entities.Add(player);

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
			if (kb.WasKeyReleased(Keys.OemTilde))
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
			//server.SendMessageToAllClients(new WorldUpdate() { /*Map = map*/ });

			// foreach client, send the player info to each other client
			foreach (var e in gameState.Entities)
			{
				//server.SendMessageToAllClientsExcept(new PlayerUpdate() { /*Position = player.Position*/ }, e);
			}
		}

		private void NetworkReceive(GameTime gameTime)
		{
			// process server queue
			foreach (var (client, msg) in server.GetReceivedMessages())
			{
				if (msg is InputUpdate networkMsg)
				{
					Logger.Debug("[NetworkInput Message] {time} {x} {y}", networkMsg.InputTimeUnixMilliseconds, networkMsg.Mouse.X, networkMsg.Mouse.Y);
					clientMousePos = new Vector2(networkMsg.Mouse.X, networkMsg.Mouse.Y);
					// input handling above, everything else below
					var entity = gameState.Entities.Where(e => e.Id == networkMsg.ClientId).Single();
					((Player)entity).Update(networkMsg, gameTime);
				}

				if (msg is LoginRequest loginMsg)
				{
					if (server.GetConnectedEntities().OfType<Player>().Any(p => p.Username == loginMsg.Username))
					{
						//server.SendMessageToClient()
						// user already logged in
						Logger.Warning("[NetworkReceive] Player already logged in: {user}", loginMsg.Username);
						break;
					}

					// todo: look up player data from a database, eg:
					// var player = db.LoadPlayer(loginMsg.Username, loginMsg.Password);
					// for now we'll just always force-make a new player
					var uid = Guid.NewGuid();
					client.ControllingEntity = new Player() { Username = loginMsg.Username, Password = loginMsg.Password, Id = uid };
					client.IsLoggedIn = true;

					Logger.Information("[NetworkReceive] Player logged in: {user}", loginMsg.Username);

					_ = client.QueueMessage(new LoginResponse() { ClientId = uid });
				}

				if (msg is LogoutRequest logoutMsg)
				{
					Logger.Information("[NetworkReceive] Player logged out: {user}", logoutMsg.Username);
					client.IsLoggedIn = false;
				}

				if (msg is KeepAliveMessage keepAliveMsg)
				{
					Logger.Debug("[NetworkReceive] [KeepAlive] ClientId={clientId}", keepAliveMsg.ClientId);
				}
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.LightGray);

			sb.Begin();

			var scale = 1f / 4f;
			MapRenderer.Draw(sb, gameState.Map, (int)(tileSize * scale));

			foreach (var e in gameState.Entities)
			{
				EntityRenderer.Draw(sb, e, scale);
			}

			sb.DrawPoint(clientMousePos, Color.White, 3f);

			sb.End();

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			if (renderLog)
			{
				InMemorySinkRenderer.Draw(logsink, sb, 10, 10);
			}

			sb.End();

			GuiRenderer.BeginLayout(gameTime);
			RenderImGui();
			GuiRenderer.EndLayout();

			base.Draw(gameTime);
		}

		public void RenderImGui()
		{
			ImGui.Text($"Clients={server.ClientCount}");
			foreach (var c in server.Clients)
			{
				ImGui.BulletText(c.ConnectionDetails);
			}
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
		public static void Draw(SpriteBatch sb, IEntity entity, float scale)
		{
			sb.FillRectangle(new RectangleF(entity.Position.X * scale, entity.Position.X * scale, entity.GetSize().X * scale, entity.GetSize().Y * scale), Color.Chocolate);
			sb.DrawDebugStringCentered(GameServices.Fonts.First().Value, entity.DisplayName, entity.Position, Color.White);
		}
	}
}