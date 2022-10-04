using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Odyssey.Generation
{
	public static class Textures
	{
		public static Texture2D GenGrassTexture(GraphicsDevice gd, int width, int height)
		{
			var tex = new RenderTarget2D(gd, width, height);
			gd.SetRenderTarget(tex);
			var sb = new SpriteBatch(gd);

			sb.Begin();
			sb.FillRectangle(0, 0, width, height, Color.Green);
			sb.End();

			gd.SetRenderTarget(null);
			return tex;
		}
	}
}
