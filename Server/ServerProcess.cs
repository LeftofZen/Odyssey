using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.ECS;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;
using Odyssey.Entities;
using Odyssey.Logging;
using Odyssey.Messaging;
using Odyssey.Networking;
using Odyssey.Noise;
using Odyssey.World;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

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

		private const int initialMapWidth = 128;
		private const int initialMapHeight = 128;
		private const int tileSize = 32;
		private NoiseParams NoiseSettings { get; } = new NoiseParams();
		private NoiseParams PreviousNoiseSettings { get; set; } = new NoiseParams();

		#endregion

		private ImGuiRenderer GuiRenderer;

		//public Vector2 clientMousePos = Vector2.Zero;

		public ServerProcess()
		{
			ClearLogs();

			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1920,
				PreferredBackBufferHeight = 1080
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
			NoiseSettings.NoiseSizeX = initialMapWidth;
			NoiseSettings.NoiseSizeY = initialMapHeight;
			gameState.Map = new Map(initialMapWidth, initialMapHeight, NoiseHelpers.CreateNoise2D(NoiseSettings))
			{
				TileSize = 32
			};

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
			MouseExtended.Update();
			KeyboardExtended.Update();

			var kb = KeyboardExtended.GetState();
			if (kb.WasKeyReleased(Keys.OemTilde))
			{
				renderLog = !renderLog;
			}

			server.Update(gameTime);

			// these can be done concurrently and should be async
			{
				NetworkReceive(gameTime);
				NetworkSend();
			}

			base.Update(gameTime);
		}

		private void NetworkSend()
		{
			//var worldUpdate = new WorldUpdate()
			//{
			//	Width = gameState.Map.Width,
			//	Height = gameState.Map.Height,
			//	TileSize = gameState.Map.TileSize,
			//	//Map = gameState.Map.GetData()
			//};
			server.SendMessageToAllClients(new GameStateUpdate() { Width = gameState.Map.Width, Height = gameState.Map.Height, TileSize = gameState.Map.TileSize });

			// foreach client, send the player info to each other client
			//foreach (var e in gameState.Entities)
			//{
				//server.SendMessageToAllClientsExcept(new PlayerUpdate() { /*Position = player.Position*/ }, e);
			//}
		}

		private void NetworkReceive(GameTime gameTime)
		{
			// process server queue
			foreach (var (client, msg) in server.GetReceivedMessages())
			{
				if (msg is InputUpdate networkMsg)
				{
					if (!client.IsLoggedIn)
					{
						Logger.Warning($"[NetworkReceive] Client \"{client.ConnectionDetails}\" not logged in");
						continue;
					}

					Logger.Debug("[NetworkInput Message] {time} {x} {y}", networkMsg.InputTimeUnixMilliseconds, networkMsg.Mouse.X, networkMsg.Mouse.Y);
					//
					//client.ControllingEntity.Position = new Vector2(networkMsg.Mouse.X, networkMsg.Mouse.Y);
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
					var displayName = "[Player]" + loginMsg.Username;
					var player = new Player()
					{
						Username = loginMsg.Username,
						Password = loginMsg.Password,
						Id = uid,
						DisplayName = displayName,
						Position = new Vector2(10, 10)
					};

					client.ControllingEntity = player;
					gameState.Entities.Add(player);

					_ = client.QueueMessage(new LoginResponse() { ClientId = uid, DisplayName = displayName, X = player.Position.X, Y = player.Position.Y });

					Logger.Information("[NetworkReceive] Player logged in: {user}", loginMsg.Username);
					client.IsLoggedIn = true;
				}

				if (msg is LogoutRequest logoutMsg)
				{
					_ = client.QueueMessage(new LogoutResponse() { ClientId = logoutMsg.ClientId });

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

			var xOffset = 128;
			var yOffset = 256;
			var scale = 1f / 8f;
			MapRenderer.Draw(sb, gameState.Map, xOffset, yOffset, (int)(tileSize * scale));

			foreach (var e in gameState.Entities)
			{
				EntityRenderer.Draw(sb, e, scale);
			}

			//sb.DrawPoint(clientMousePos, Color.White, 3f);

			sb.End();

			sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			if (renderLog)
			{
				InMemorySinkRenderer.Draw(logsink, sb, 0, 0);
			}

			sb.End();

			GuiRenderer.BeginLayout(gameTime);
			RenderImGui();
			GuiRenderer.EndLayout();

			base.Draw(gameTime);
		}

		public void RenderImGuiClient(OdysseyClient client)
		{
			if (client?.Connected == true)
			{
				ImGui.BulletText(client.ConnectionDetails);
				if (client.ControllingEntity != null)
				{
					ImGui.BulletText($"Display name: \"{client.ControllingEntity.DisplayName}\"");
					ImGui.BulletText($"Position: {client.ControllingEntity.Position}");
					ImGui.BulletText($"Velocity: {client.ControllingEntity.Velocity}");
					ImGui.BulletText($"Acceleration: {client.ControllingEntity.Acceleration}");

					if (client.ControllingEntity is Player p)
					{
						ImGui.BulletText($"Username: \"{p.Username}\"");
						ImGui.BulletText($"Password: \"{p.Password}\"");
					}
				}
			}
		}

		public void RenderImGui()
		{
			ImGui.Text($"Clients={server.ClientCount}");
			var disconnectList = new List<OdysseyClient>();
			foreach (var c in server.Clients)
			{
				RenderImGuiClient(c);

				if (ImGui.Button("Disconnect"))
				{
					disconnectList.Add(c);
				}

				if (ImGui.Button("Send chat"))
				{
					_ = c.QueueMessage(new ChatMessage() { Message = "Hello from the server!" });
				}
			}

			foreach (var c in disconnectList)
			{
				gameState.Entities.RemoveAll(x => x.Id == c.ControllingEntity.Id);
				server.DisconnectClient(c);
			}
		}
	}
}