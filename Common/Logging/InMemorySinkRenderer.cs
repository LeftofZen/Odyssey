using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Odyssey.Render;
using Serilog.Events;
using Serilog.Parsing;

namespace Odyssey.Logging
{
	//public record RenderParams(SpriteBatch sb, SpriteFont font, int x, int y);

	public static class InMemorySinkRenderer
	{
		public static void Draw(InMemorySink sink, SpriteBatch sb, int x, int y)
		{
			sb.FillRectangle(0, 0, sb.GraphicsDevice.Viewport.Width, 216, new Color(Color.DarkGray, 0.5f));

			var yInc = y;
			// logs
			//foreach (var log in sink.Events.TakeLast(20).Reverse())
			foreach (var log in sink.Events.TakeLast(10))
			{
				//DrawLogLine(log, sb, GameServices.Fonts["Calibri"], x, y);
				var renderedStringSize = RenderLine(log, sb, GameServices.Fonts["Calibri"], new Vector2(x, yInc));
				yInc += 20;
			}
		}

		public static Vector2 RenderLine(LogEvent logEvent, SpriteBatch sb, SpriteFont font, Vector2 pos)
		{
			if (font is null)
			{
				return Vector2.Zero;
			}
			var timestamp = logEvent.Timestamp.ToString();
			sb.DrawDebugStringLeftAligned(font, timestamp, pos, Color.Firebrick, 1);
			pos += font.MeasureString(timestamp).SetY(0);

			foreach (var token in logEvent.MessageTemplate.Tokens)
			{
				if (token is TextToken tt)
				{
					pos += RenderTextToken(tt, sb, font, pos).SetY(0);
				}
				else
				{
					var pt = (PropertyToken)token;
					pos += RenderPropertyToken(pt, logEvent.Properties, sb, font, pos).SetY(0);
				}
			}
			return pos;
		}

		private static Vector2 RenderTextToken(TextToken tt, SpriteBatch sb, SpriteFont font, Vector2 pos)
		{
			//using (_theme.Apply(output, ConsoleThemeStyle.Text, ref count))
			//	output.Write(tt.Text);
			sb.DrawDebugStringLeftAligned(font, tt.Text, pos, Color.CornflowerBlue, 1);

			return font.MeasureString(tt.Text);
		}

		private static Vector2 RenderPropertyToken(PropertyToken pt, IReadOnlyDictionary<string, LogEventPropertyValue> properties, SpriteBatch sb, SpriteFont font, Vector2 pos)
		{
			if (!properties.TryGetValue(pt.PropertyName, out var propertyValue))
			{
				//using (_theme.Apply(output, ConsoleThemeStyle.Invalid, ref count))
				//	output.Write(pt.ToString());
				sb.DrawDebugStringLeftAligned(font, pt.ToString(), pos, Color.LawnGreen, 1);
				return font.MeasureString(pt.ToString());
			}

			if (!pt.Alignment.HasValue)
			{
				//return RenderValue(_theme, _valueFormatter, propertyValue, output, pt.Format);
				if (propertyValue is ScalarValue sv)
				{
					var s1 = $"{pt.PropertyName}=";
					sb.DrawDebugStringLeftAligned(font, s1, pos, Color.Gray, 1);
					var l1 = font.MeasureString(s1);

					sb.DrawDebugStringLeftAligned(font, propertyValue.ToString(), pos + l1.SetY(0), Color.Aquamarine, 1);
					var l2 = font.MeasureString(propertyValue.ToString());

					return l1 + l2;
				}
			}

			var valueOutput = new StringWriter();

			//if (!_theme.CanBuffer)
			//	return RenderAlignedPropertyTokenUnbuffered(pt, output, propertyValue);

			//var invisibleCount = RenderValue(_theme, _valueFormatter, propertyValue, valueOutput, pt.Format);

			var value = valueOutput.ToString();

			//if (value.Length - invisibleCount >= pt.Alignment.Value.Width)
			//{
			//	Odyssey.Render.String.DrawDebugStringLeftAligned(rp.sb, rp.font, propertyValue.ToString(), new Vector2(rp.x, rp.y), Color.MediumPurple, 1);
			//output.Write(value);
			//}
			//else
			//{
			//	Padding.Apply(output, value, pt.Alignment.Value.Widen(invisibleCount));
			//}

			return Vector2.Zero;
		}

		//static int RenderAlignedPropertyTokenUnbuffered(PropertyToken pt, TextWriter output, LogEventPropertyValue propertyValue, RenderParams rp)
		//{
		//	if (pt.Alignment == null) throw new ArgumentException("The PropertyToken should have a non-null Alignment.", nameof(pt));

		//	var valueOutput = new StringWriter();
		//	RenderValue(NoTheme, _unthemedValueFormatter, propertyValue, valueOutput, pt.Format);

		//	var valueLength = valueOutput.ToString().Length;
		//	if (valueLength >= pt.Alignment.Value.Width)
		//	{
		//		return RenderValue(_theme, _valueFormatter, propertyValue, output, pt.Format);
		//	}

		//	if (pt.Alignment.Value.Direction == AlignmentDirection.Left)
		//	{
		//		var invisible = RenderValue(_theme, _valueFormatter, propertyValue, output, pt.Format);
		//		Padding.Apply(output, string.Empty, pt.Alignment.Value.Widen(-valueLength));
		//		return invisible;
		//	}

		//	Padding.Apply(output, string.Empty, pt.Alignment.Value.Widen(-valueLength));
		//	return RenderValue(_theme, _valueFormatter, propertyValue, output, pt.Format);
		//}

		//static int RenderValue(ConsoleTheme theme, ThemedValueFormatter valueFormatter, LogEventPropertyValue propertyValue, TextWriter output, string? format, RenderParams rp)
		//{
		//	if (_isLiteral && propertyValue is ScalarValue sv && sv.Value is string)
		//	{
		//		var count = 0;
		//		using (theme.Apply(output, ConsoleThemeStyle.String, ref count))
		//			output.Write(sv.Value);
		//		return count;
		//	}

		//	return valueFormatter.Format(propertyValue, output, format, _isLiteral);
		//}

		public static void DrawLogLine(LogEvent log, SpriteBatch sb, SpriteFont font, int x, int y)
		{
			//log.
			var pos = new Vector2(x, y);
			foreach (var property in log.Properties)
			{
				var s = property.Key + "_" + property.Value;
				var l = font.MeasureString(s);
				sb.DrawDebugStringLeftAligned(font, s, pos, Color.CornflowerBlue, 1);
				pos = new Vector2(pos.X + l.X, pos.Y);
			}

			//var l1 = font.MeasureString(log.initialTime.ToString());
			//var l2 = font.MeasureString(log.message);
			//var l3 = font.MeasureString(log.millisondsToDisplay.ToString());

			//Render.String.DrawDebugStringLeftAligned(sb, font, log.initialTime.ToString(), new Vector2(x, y), Color.Black, 1);
			//Render.String.DrawDebugStringLeftAligned(sb, font, log.message, new Vector2(x + l1.X, y), Color.WhiteSmoke, 1);
			//Render.String.DrawDebugStringLeftAligned(sb, font, log.millisondsToDisplay.ToString(), new Vector2(x + l1.X + l2.X, y), Color.CornflowerBlue, 1);
		}
	}
}
