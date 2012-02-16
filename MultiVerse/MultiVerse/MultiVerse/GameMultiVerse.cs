using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiVerse
{
	public delegate void Action();

	public sealed class GameMultiVerse : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics = null;

		public SpriteBatch spriteBatch = null;
		public SpriteFont spriteFont = null;

		public const int gfxWndWidth = 1024;
		public const int gfxWndHeight = 768;

		public const int menuSpacingV = 80;

		public const int levelWidth = 20;
		public const int levelHeight = 20;

		public bool clearBackBuffer = true;
		public Color clearColorBackBuffer = Color.CornflowerBlue;

		private GameState pushed = null;
		private GameState currentGameState = null;
		public GameState CurrentGameState
		{
			get { return currentGameState; }
			set
			{
				pushed = value;
			}
		}

		public readonly string[] skyBoxTextures = new string[] {"easter",
																"gray_matter",
																"ice_field",
																"lemon_lime",
																"milk_chocolate",
																"solar_bloom",
																"thick_rb"};

		public static int FOV = 75;

		public static int cameraViewDistance = 1000;

		public static float skyboxSize = 500;

		public bool inverseY = true;
		public bool showSpectrum = true;

		private static readonly GameMultiVerse instance = new GameMultiVerse();
		public static GameMultiVerse Instance
		{
			get
			{
				return instance;
			}
		}

		protected override void OnExiting(object sender, System.EventArgs args)
		{
			base.OnExiting(sender, args);

			// mivel egy�bk�nt csak a v�gs� mem�ria felszabad�t�skor �llna meg, ami sajnos nem egyb�l a kil�p�skor van
			//	(�gy a zene tov�bb sz�l egy ideig az ablak elt�n�se ut�n)
			SongUtils.Instance.ReleaseSong();
		}

		private GameMultiVerse()
		{
			graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";

			graphics.PreferredBackBufferWidth = gfxWndWidth;
			graphics.PreferredBackBufferHeight = gfxWndHeight;

			Window.Title = "MultiVerse ALPHA";
		}

		protected override void Initialize()
		{
			base.Initialize();
		}

		protected override void LoadContent()
		{
			spriteBatch = new SpriteBatch(GameMultiVerse.Instance.GraphicsDevice);
			spriteFont = GameMultiVerse.Instance.Content.Load<SpriteFont>("Fonts/gamefont");

			currentGameState = new GameState_MainMenu();
			currentGameState.Start();
		}

		protected override void UnloadContent()
		{
			Content.Unload();
		}

		protected override void Update(GameTime gameTime)
		{
			if (pushed != null)
			{
				if (currentGameState != null)
					currentGameState.Exit();

				currentGameState = pushed;
				pushed.Start();
				pushed = null;
			}

			InputManager.Update();

			FadeManager.Update(gameTime);

			MenuManager.Update(gameTime);

			currentGameState.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			if (clearBackBuffer)
				GraphicsDevice.Clear(clearColorBackBuffer);

			currentGameState.Draw(gameTime);

			MenuManager.Draw(gameTime);

			FadeManager.Draw(gameTime);
			
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			string text = "Created by: Matyi Csapo";
			spriteBatch.DrawString(spriteFont,
									text,
									new Vector2(gfxWndWidth - 10,
										gfxWndHeight - 40),
									Color.Cyan,
									0,
									Instance.spriteFont.MeasureString(text),
									.5f,
									SpriteEffects.None,
									0);
			text = "matyicsapo@gmail.com";
			spriteBatch.DrawString(spriteFont,
									text,
									new Vector2(gfxWndWidth - 10,
										gfxWndHeight - 10),
									Color.Cyan,
									0,
									Instance.spriteFont.MeasureString(text),
									.5f,
									SpriteEffects.None,
									0);

			spriteBatch.End();

			// ezek vissza�ll�t�sa sz�ks�ges a 3D r�szek(pl.: skybox) megfelel�(norm�l) megjelen�t�s�hez
			// mivel a 2D rajzol�s viszont m�s be�ll�t�sokat ig�nyel
			GraphicsDevice.BlendState = BlendState.Opaque;
			GraphicsDevice.DepthStencilState = DepthStencilState.Default;

			base.Draw(gameTime);
		}
	}
}
