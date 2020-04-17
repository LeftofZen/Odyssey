using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Monogame3DTest1
{
	/// <summary>
	/// This is the main type for your game.
	/// </summary>
	public class Game1 : Game
	{
		GraphicsDeviceManager graphics;

		Vector3 camTarget;
		Vector3 camPosition;

		Matrix projectionMatrix;
		Matrix viewMatrix;
		Matrix worldMatrix;

		BasicEffect basicEffect;

		// Geometric Info
		VertexPositionColor[] triangleVertices;

		VertexPositionColor[] cubeVertices;
		short[] cubeIndices;

		VertexBuffer vertexBuffer;
		IndexBuffer indexBuffer;

		// Camera Orbit
		bool orbit;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
		}

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			camTarget = new Vector3(0f, 0f, 0f);
			camPosition = new Vector3(0f, 2f, 4f);

			projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45f), GraphicsDevice.DisplayMode.AspectRatio, 1f, 100f);
			viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);
			worldMatrix = Matrix.CreateWorld(camTarget, Vector3.Forward, Vector3.Up);

			basicEffect = new BasicEffect(GraphicsDevice);
			basicEffect.Alpha = 1.0f;
			basicEffect.VertexColorEnabled = true;
			basicEffect.LightingEnabled = false;

			cubeVertices = new VertexPositionColor[8];
			cubeVertices[0] = new VertexPositionColor(new Vector3(0f, 0f, 0f), Color.White);
			cubeVertices[1] = new VertexPositionColor(new Vector3(0f, 1f, 0f), Color.Red);
			cubeVertices[2] = new VertexPositionColor(new Vector3(1f, 1f, 0f), Color.Green);
			cubeVertices[3] = new VertexPositionColor(new Vector3(1f, 0f, 0f), Color.Blue);
			cubeVertices[4] = new VertexPositionColor(new Vector3(0f, 0f, -1f), Color.Yellow);
			cubeVertices[5] = new VertexPositionColor(new Vector3(0f, 1f, -1f), Color.Purple);
			cubeVertices[6] = new VertexPositionColor(new Vector3(1f, 0f, -1f), Color.Cyan);
			cubeVertices[7] = new VertexPositionColor(new Vector3(1f, 1f, -1f), Color.Black);

			cubeIndices = new short[36]
			{
				// front
				0, 1, 2,
				2, 3, 0,

				// back
				6, 7, 5,
				4, 6, 5,

				//// top
				1, 5, 7,
				7, 2, 1,

				// bottom
				3, 0, 4,
				4, 6, 3,

				// left
				4, 5, 1,
				1, 0, 4,

				// right
				3, 2, 7,
				7, 6, 3,
			};

			vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), cubeVertices.Length, BufferUsage.WriteOnly);
			vertexBuffer.SetData(cubeVertices);

			indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), cubeIndices.Length, BufferUsage.WriteOnly);
			indexBuffer.SetData(cubeIndices);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
		}

		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			var camSpeed = 0.1f;

			if (Keyboard.GetState().IsKeyDown(Keys.Left))
			{
				camPosition.X -= camSpeed;
				camTarget.X -= camSpeed;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Right))
			{
				camPosition.X += camSpeed;
				camTarget.X += camSpeed;
			}
			if (Keyboard.GetState().IsKeyDown(Keys.Up))
			{
				camPosition.Y += camSpeed;
				camTarget.Y += camSpeed;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.Down))
			{
				camPosition.Y -= camSpeed;
				camTarget.Y -= camSpeed;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.OemPlus))
			{
				camPosition.Z -= camSpeed;
				camTarget.Z -= camSpeed;
			}
			else if (Keyboard.GetState().IsKeyDown(Keys.OemMinus))
			{
				camPosition.Z += camSpeed;
				camTarget.Z += camSpeed;
			}

			if (Keyboard.GetState().IsKeyDown(Keys.Space))
			{
				orbit = !orbit;
			}
			if (orbit)
			{
				Matrix rotationMatrix = Matrix.CreateRotationY(MathHelper.ToRadians(1f));
				camPosition = Vector3.Transform(camPosition, rotationMatrix);
			}

			viewMatrix = Matrix.CreateLookAt(camPosition, camTarget, Vector3.Up);

			// TODO: Add your update logic here

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			basicEffect.View = viewMatrix;
			basicEffect.World = worldMatrix;
			basicEffect.Projection = projectionMatrix;

			GraphicsDevice.Clear(Color.CornflowerBlue);

			GraphicsDevice.Indices = indexBuffer;
			GraphicsDevice.SetVertexBuffer(vertexBuffer);
			GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferEnable = true };

			var rasterizerState = new RasterizerState();
			rasterizerState.CullMode = CullMode.CullCounterClockwiseFace;

			GraphicsDevice.RasterizerState = rasterizerState;

			foreach (var v in basicEffect.CurrentTechnique.Passes)
			{
				v.Apply();
				GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
			}

			base.Draw(gameTime);
		}
	}
}
