using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;
using MonoGame.ImGuiNet;
using Odyssey.Entities;
using Odyssey.Logging;
using Odyssey.Messaging;
using Odyssey.Networking;
using Odyssey.Render;
using Odyssey.World;
using Serilog;
using Serilog.Exceptions;
using System;
using System.Threading;

namespace Odyssey.Client
{
	public class ClientProcess : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch sb;
		private OdysseyClient client;
		private Guid Id;

		private ILogger Logger;
		private InMemorySink logsink;
		private bool renderLog = true;

		// game state
		public Map map;
		public Player player;

		// rendering
		private ImGuiRenderer GuiRenderer;
		public Camera camera;

		public ClientProcess()
		{
			Id = Guid.NewGuid();
			ClearLogs();

			graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1080,
				PreferredBackBufferHeight = 768
			};

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Window.Title = "Odyssey (client)";

			client = new OdysseyClient(Networking.Constants.DefaultHostname, Networking.Constants.DefaultPort);
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

		protected override void Initialize()
		{
			camera = new Camera(GraphicsDevice.Viewport);

			// imgui
			GuiRenderer = new ImGuiRenderer(this);
			GuiRenderer.RebuildFontAtlas();

			player = new Player()
			{
				DisplayName = "Left of Zen",
				Username = "bob",
				Password = "foo",
			};

			// click button on imgui
			//ConnectToServer();

			base.Initialize();
		}

		public void ConnectToServer()
		{
			if (!client.Connected)
			{
				_ = client.Connect();
				Logger.Debug("[ClientProcess::ConnectToServer] {connected}", client.Connected);
			}
		}

		public void DisconnectFromServer()
		{
			Logger.Debug("[ClientProcess::DisconnectFromServer] {connected}", client?.Connected);
			if (client?.Connected == true)
			{
				client.Disconnect();
			}
		}

		protected override void LoadContent()
		{
			sb = new SpriteBatch(GraphicsDevice); // TODO: use this.Content to load your game content here
			GameServices.LoadContent(Content, GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
			}

			var kb = KeyboardExtended.GetState();
			if (kb.WasKeyReleased(Keys.OemTilde))
			{
				renderLog = !renderLog;
			}

			NetworkSend();
			NetworkReceive();

			if (player != null)
			{
				camera.Follow(player.Position);
			}
			camera.UpdateCamera(GraphicsDevice.Viewport);

			base.Update(gameTime);
		}

		private void NetworkReceive()
		{
			if (client == null)
			{
				return;
			}

			while (client.TryDequeueMessage(out var dmsg))
			{
				Logger.Debug("[ClientProcess::NetworkReceive] {to} {msg}", player.Username, dmsg.hdr.Type);
				if (dmsg.msg is LoginResponse loginResponse)
				{
					// if loginResponse == successful
					player.Id = loginResponse.ClientId;
					client.ControllingEntity = player;
					client.LoginMessageInFlight = false;
					client.IsLoggedIn = true;
					Logger.Information("[ClientProcess::NetworkReceive][LoginResponse] \"{player}\" logged in with {id}", player.DisplayName, player.Id);
				}

				if (dmsg.msg is LogoutResponse)
				{
					// if loginResponse == successful
					var oldId = player.Id;
					player.Id = Guid.Empty;
					client.ControllingEntity = null;
					client.LogoutMessageInFlight = false;
					client.IsLoggedIn = false;
					Logger.Information("[ClientProcess::NetworkReceive][LogoutResponse] \"{player}\" logged out with {id}", player.DisplayName, oldId);
				}

				if (dmsg.msg is ChatMessage chatMessage)
				{
					Logger.Information($"[ClientProcess::NetworkReceive][Chat][{chatMessage.ClientId}] {chatMessage.Message}");
				}
			}

		}

		public void Login()
		{
			if (!client.IsLoggedIn && !client.LoginMessageInFlight)
			{
				_ = client.Login(player.Username, player.Password);
			}
		}

		public void Logout()
		{
			if (client.IsLoggedIn && !client.LoginMessageInFlight)
			{
				_ = client.Logout(player.Username);
			}
		}

		static int count = 0;

		private void NetworkSend()
		{
			if (client?.Connected != true)
			{
				return;
			}

			//Login();

			var clientInput = new InputUpdate()
			{
				Mouse = MouseInputData.FromMouseState(Mouse.GetState()),
				Keyboard = KeyboardInputData.FromKeyboardState(Keyboard.GetState()),
				Gamepad = GamePadInputData.FromGamePadState(GamePad.GetState(PlayerIndex.One)),
				InputTimeUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
				ClientId = player.Id,
			};

			// this limits input sending to only when keys are pressed - not great
			// for when mouse/ gamepad input happens! but we'll add that later
			// for now its fine just to send every frame
			if (Keyboard.GetState().GetPressedKeys().Length > 0)
			{
				if (!client.QueueMessage(clientInput))
				{
					Logger.Error("[ClientProcess::NetworkSend] Couldn't send message: {type}", nameof(NetworkMessageType.InputUpdate));
				}
			}

			if (client.PendingMessages == 0 && (count++ % 60) == 0) // once per second
			{
				Logger.Debug("[ClientProcess::NetworkSend] No pending messages, will send keepalive instead");
				_ = client.QueueMessage(new KeepAliveMessage() { ClientId = Id, Timestamp = DateTime.Now.Ticks });
			}

			client.FlushMessages();
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.SteelBlue);

			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.Transform);

			// draw world
			if (map.IsInitialised)
			{
				map.Draw(sb, gameTime, camera);

				// draw highlighted tile
				var mousePos = Mouse.GetState().Position.ToVector2();
				mousePos = Render.ScreenToWorldSpace(mousePos, camera.Transform);
				Render.DrawTileAlignedBox(sb, mousePos, map.TileSize, Color.Yellow, map.TileSize);
			}

			// draw animals
			//foreach (var a in animals)
			//{
			//	a.Draw(sb, gameTime);
			//	Render.DrawTileAlignedBox(a.Position, tileSize, Color.LightBlue);
			//	Render.DrawBoundingBox(a);
			//	if (camera.Zoom >= 1)
			//	{
			//		Render.DrawDebugStringCentered(sb, GameServices.Fonts["Calibri"], a.Name, a.Position - new Vector2(0, a.Size.Y / 2), Color.White);
			//	}
			//}

			// draw player
			if (player != null)
			{
				player.Draw(sb, gameTime);
				Render.DrawTileAlignedBox(sb, player.Position, map.TileSize, Color.Red, map.TileSize);
				Render.DrawBoundingBox(sb, player);
				if (camera.Zoom >= 1)
				{
					sb.DrawDebugStringCentered(GameServices.Fonts["Calibri"], player.DisplayName, player.Position - new Vector2(0, player.Size.Y / 2), Color.White);
				}

			}

			if (client != null)
			{
				sb.DrawDebugStringCentered(GameServices.Fonts["Calibri"], client.IsLoggedIn.ToString(), new Vector2(20, 300), Color.White);
			}

			//DrawMap(sb, mapLookup["map1"]);
			//DrawDebugString(sb, fontLookup["Calibri"], $"DrawCount={drawCount}", new Vector2(8, 8));
			//sb.Draw(texLookup["terrain"], Vector2.Zero, Color.White);
			sb.End();

			// non-camera things, ie ui things
			//sb.Begin();

			//if (camera.Zoom < 1)
			//{
			//	foreach (var a in animals)
			//	{
			//		Render.DrawDebugStringCentered(sb, GameServices.Fonts["Calibri"], a.Name, Vector2.Transform(a.Position - new Vector2(0, a.Size.Y / 2), camera.Transform), Color.White);
			//	}

			//	Render.DrawDebugStringCentered(sb, GameServices.Fonts["Calibri"], player.Name, Vector2.Transform(player.Position - new Vector2(0, player.Size.Y / 2), camera.Transform), Color.White);
			//}

			// logs
			//var logStartY = 100;
			//foreach (var log in logMemSink.Events.TakeLast(40))
			//{
			//	DrawDebugStringLeftAligned(sb, GameServices.Fonts["Calibri"], log.message, new Vector2(100, logStartY), Color.Red, 1);
			//	logStartY += 20;
			//}

			//sb.End();

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
			if (client != null)
			{
				ImGui.BulletText(client.ConnectionDetails);
			}

			if (ImGui.Button("Connect"))
			{
				ConnectToServer();
			}
			if (ImGui.Button("Disconnect"))
			{
				DisconnectFromServer();
			}
			if (ImGui.Button("Clear Logs"))
			{
				ClearLogs();
			}

			if (client?.Connected == true)
			{
				if (ImGui.Button("Login"))
				{
					Login();
				}
				if (ImGui.Button("Logout"))
				{
					Logout();
				}
			}
		}
	}
}