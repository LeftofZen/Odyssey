﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.ImGui;
using Odyssey.Entities;
using Odyssey.Logging;
using Odyssey.Network;
using Odyssey.Networking;
using Odyssey.Networking.Messages;
using Odyssey.World;
using Serilog;

namespace Odyssey.Client
{
	public class ClientProcess : Game
	{
		private GraphicsDeviceManager graphics;
		private SpriteBatch sb;
		private OdysseyClient client;
		private InMemorySink logsink;

		// game state
		public Map map;
		public Player player;

		// rendering
		public ImGUIRenderer GuiRenderer; //This is the ImGuiRenderer
		public Camera camera;

		public ClientProcess()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;

			client = new OdysseyClient(Constants.DefaultHostname, Constants.DefaultPort);
			logsink = new InMemorySink();

			Log.Logger = new LoggerConfiguration()
				//.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}") // https://github.com/serilog/serilog/wiki/Formatting-Output
				.WriteTo.Sink(logsink)
				.MinimumLevel.Debug()
				.CreateLogger();
		}

		protected override void Initialize()
		{
			// ui
			camera = new Camera(GraphicsDevice.Viewport);
			GuiRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			sb = new SpriteBatch(GraphicsDevice);// TODO: use this.Content to load your game content here
			GameServices.LoadContent(Content, GraphicsDevice);
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			{
				Exit();
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
			client.ReadMessages();
			foreach (var msg in client.MessageQueue)
			{
				switch (msg.Type)
				{
					case NetworkMessageType.WorldUpdate:
						map = ((WorldUpdate)msg).Map;
						break;
					case NetworkMessageType.PlayerUpdate:
						player.SetPosition(((PlayerUpdate)msg).Position);
						break;
				}
			}
		}

		private void NetworkSend()
		{
			var clientInput = new NetworkInput()
			{
				Mouse = Mouse.GetState(),
				Keyboard = Keyboard.GetState(),
				Gamepad = GamePad.GetState(PlayerIndex.One),
				InputTimeUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
				//Name = "Left of Zen",
			};

			// this limits input sending to only when keys are pressed - not great
			// for when mouse/ gamepad input happens! but we'll add that later
			// for now its fine just to send every frame
			if (Keyboard.GetState().GetPressedKeys().Length > 0)
			{
				client.SendMessage(NetworkMessageType.NetworkInput, clientInput);
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.SteelBlue);

			sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, camera.Transform);

			// draw world
			if (map != null)
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
					Odyssey.Render.String.DrawDebugStringCentered(sb, GameServices.Fonts["Calibri"], player.Name, player.Position - new Vector2(0, player.Size.Y / 2), Color.White);
				}
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

			//Render.DrawImGui(GuiRenderer, gameTime);

			base.Draw(gameTime);
		}
	}
}