using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Odyssey.Render
{
	public static class String
	{
		public static void DrawDebugStringCentered(SpriteBatch sb, SpriteFont sf, string str, Vector2 pos, Color color, float scale = 1f)
		{
			var size = sf.MeasureString(str);
			sb.DrawString(sf, str, pos - (size / 2) + Vector2.One, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			sb.DrawString(sf, str, pos - (size / 2), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}

		public static void DrawDebugStringLeftAligned(SpriteBatch sb, SpriteFont sf, string str, Vector2 pos, Color color, float scale = 1f)
		{
			var size = sf.MeasureString(str);
			sb.DrawString(sf, str, pos + Vector2.One, Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
			sb.DrawString(sf, str, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}
}
