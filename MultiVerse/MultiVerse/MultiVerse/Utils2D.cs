using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace MultiVerse
{
	static class Utils2D
	{
		static Texture2D lineTexture = null;

		static Utils2D()
		{
			lineTexture = new Texture2D(GameMultiVerse.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			lineTexture.SetData(new[] { Color.White });
		}

		public static void DrawPlot(float[] samples, Vector2 offset, Vector2 scale, float maxHeight)
		{
			for (int i = 1; i < samples.Length; i++)
			{
				DrawLine(1, Color.Green,
					new Vector2(scale.X * (i - 1) + offset.X, maxHeight - offset.Y
											- ((maxHeight - offset.Y) * samples[i - 1]) * scale.Y),
					new Vector2(scale.X * i + offset.X, maxHeight - offset.Y
											- ((maxHeight - offset.Y) * samples[i]) * scale.Y)
				);
			}
		}

		public static void DrawPlot(bool[] samples, Vector2 offset, Vector2 scale, float maxHeight)
		{
			for (int i = 1; i < samples.Length; i++)
			{
				float xLast = samples[i - 1] ? 1 : 0;
				float xThis = samples[i] ? 1 : 0;

				DrawLine(1, Color.Green,
					new Vector2(scale.X * (i - 1) + offset.X, maxHeight - offset.Y
											- ((maxHeight - offset.Y) * xLast) * scale.Y),
					new Vector2(scale.X * i + offset.X, maxHeight - offset.Y
											- ((maxHeight - offset.Y) * xThis) * scale.Y)
				);
			}
		}

		public static void DrawLine(float width, Color color, Vector2 point1, Vector2 point2)
		{
			float angle = (float)Math.Atan2(point2.Y - point1.Y, point2.X - point1.X);
			float length = Vector2.Distance(point1, point2);

			GameMultiVerse.Instance.spriteBatch.Draw(lineTexture, point1, null, color,
					   angle, Vector2.Zero, new Vector2(length, width),
					   SpriteEffects.None, 0);
		}
	}
}
