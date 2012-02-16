using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MultiVerse
{
	static class FadeManager
	{
		static Texture2D fadeTexture = null;

		static float fadeSecsLeft = 0;
		static bool fading = false;

		static Vector4 from;
		static Vector4 to;
		static Vector4 color;
		static float duration;

		static Action onFinished = null;

		static FadeManager()
		{
			fadeTexture = new Texture2D(GameMultiVerse.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			fadeTexture.SetData(new[] { Color.White });
		}

		public static void Fade(Color from, Color to, float duration)
		{
			fading = true;
			fadeSecsLeft = duration;
			FadeManager.duration = duration;
			FadeManager.from = new Vector4(from.R, from.G, from.B, from.A);
			FadeManager.to = new Vector4(to.R, to.G, to.B, to.A);
			color = FadeManager.from;

			onFinished = null;
		}

		public static void Fade(Color from, Color to, float duration, Action onFinished)
		{
			fading = true;
			fadeSecsLeft = duration;
			FadeManager.duration = duration;
			FadeManager.from = new Vector4(from.R, from.G, from.B, from.A);
			FadeManager.to = new Vector4(to.R, to.G, to.B, to.A);
			color = FadeManager.from;

			FadeManager.onFinished = onFinished;
		}

		public static void Update(GameTime gameTime)
		{
			if (fading)
			{
				if (fadeSecsLeft <= 0)
				{
					fading = false;

					if (onFinished != null)
					{
						onFinished();
					}
				}
				else
				{
					fadeSecsLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;

					color += (to - from) * (float)gameTime.ElapsedGameTime.TotalSeconds * (1f / duration);
				}
			}
		}

		public static void Draw(GameTime gameTime)
		{
			if (fading)
			{
				GameMultiVerse.Instance.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied);

				Color c = new Color(color); // elvileg ez is elég lenne..de nem
				c.R = (byte)color.X;
				c.G = (byte)color.Y;
				c.B = (byte)color.Z;
				c.A = (byte)color.W;
				GameMultiVerse.Instance.spriteBatch.Draw(fadeTexture,
				    new Rectangle(0, 0, GameMultiVerse.gfxWndWidth, GameMultiVerse.gfxWndHeight),
				    c);

				GameMultiVerse.Instance.spriteBatch.End();

				// ezek visszaállítása szükséges a 3D részek(pl.: skybox) megfelelő(normál) megjelenítéséhez
				// mivel a 2D rajzolás viszont más beállításokat igényel
				GameMultiVerse.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
				GameMultiVerse.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			}
		}
	}
}
