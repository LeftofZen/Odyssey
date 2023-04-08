using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Odyssey.ECS;

namespace Odyssey.Client
{
	public static class Render
	{
		public static void DrawTileAlignedBox(SpriteBatch sb, Vector2 position, int alignment, Color color, int tileSize)
		{
			position.X -= position.X % alignment;
			position.Y -= position.Y % alignment;

			//sb.Draw(
			//	GameServices.Textures["ui"],
			//	position,
			//	new Rectangle(0, 0, tileSize, tileSize),
			//	color);
		}

		public static void DrawBoundingBox(SpriteBatch sb, IEntity entity)
		{
			var pos = (entity.Position - (entity.GetSize() / 2)).ToPoint();
			sb.DrawRectangle(
				new Rectangle(pos.X, pos.Y, (int)entity.GetSize().X, (int)entity.GetSize().Y),
				Color.Blue);
		}


		public static void DrawImGui() //, Map map)
		{
			//ImGui.ShowDemo();

		}
		/*
		{
			// ImGUI
			imgui.BeginLayout(gameTime);
			if (ImGui.CollapsingHeader("Program Settings", ImGuiTreeNodeFlags.DefaultOpen))
			{
				//_ = ImGui.Checkbox("Fullscreen", ref isFullScreen);
				//if (isFullScreen != graphics.IsFullScreen)
				//{
				//	graphics.ToggleFullScreen();
				//}

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
					var currentPos = (int)MediaPlayer.PlayPosition.TotalSeconds;
					_ = ImGui.SliderInt("Position", ref currentPos, 0, (int)GameServices.Songs["farm_music"].Duration.TotalSeconds);
				}

				if (ImGui.CollapsingHeader("Noise Settings", ImGuiTreeNodeFlags.DefaultOpen))
				{
					_ = ImGui.SliderInt("Pixels", ref NoiseSettings.NoiseSize, 1, 512);
					_ = ImGui.SliderInt("Octaves", ref NoiseSettings.Octaves, 1, 8);
					_ = ImGui.SliderFloat("Initial Amplitude", ref NoiseSettings.InitialAmplitude, 0f, 1f);
					_ = ImGui.SliderFloat("Initial Frequency", ref NoiseSettings.InitialFrequency, 0f, 1f);
					_ = ImGui.SliderFloat("Lacunarity", ref NoiseSettings.Lacunarity, 0f, 4f);
					_ = ImGui.SliderFloat("Persistence", ref NoiseSettings.Persistence, 0f, 4f);
					_ = ImGui.SliderFloat("OffsetX", ref NoiseSettings.Offset.Y, -1000f, 1000f);
					_ = ImGui.SliderFloat("OffsetY", ref NoiseSettings.Offset.X, -1000f, 1000f);
					_ = ImGui.SliderFloat("RedistributionPower", ref NoiseSettings.Redistribution, 0.001f, 10f);
					_ = ImGui.Checkbox($"UseKernel={NoiseSettings.UseKernel}", ref NoiseSettings.UseKernel);
					_ = ImGui.Checkbox($"UseTerracing={NoiseSettings.UseTerracing}", ref NoiseSettings.UseTerracing);
					_ = ImGui.SliderInt("TerraceCount", ref NoiseSettings.TerraceCount, 1, 100);
				}

				if (ImGui.CollapsingHeader("Debug Data", ImGuiTreeNodeFlags.DefaultOpen))
				{
					//ImGui.Text($"Animation Offset={animationOffset}");
					ImGui.Text($"GameTime.TotalGameTime.TotalMilliseconds={gameTime.TotalGameTime.TotalMilliseconds}");
				}
			}

			imgui.EndLayout();
		}
		*/
		public static Vector2 ScreenToWorldSpace(Vector2 point, Matrix transform)
		{
			var invertedMatrix = Matrix.Invert(transform);
			return Vector2.Transform(point, invertedMatrix);
		}
	}
}
