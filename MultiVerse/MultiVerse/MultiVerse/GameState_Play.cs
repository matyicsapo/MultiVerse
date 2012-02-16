using System;
using Background;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace MultiVerse
{
	class GameState_Play : GameState
	{
		const float gameSpd = 50;

		Skybox skyBox = null;

		Camera_FollowShip camera = null;

		Ship ship = null;
		const float shipOffsetZ = -5; // ennyivel lóg előre az orra a közzépontjához képest

		List<Obstacle> asteroids = new List<Obstacle>();
		List<Obstacle> pups = new List<Obstacle>();
		Queue<Vector3> allBeatPositions = new Queue<Vector3>();

		List<Obstacle> blackList = new List<Obstacle>();

		public string musicFile = "";
		float frequency = 0;

		readonly float levelLength = 0;

		Vector2 plotOffset = new Vector2(GameMultiVerse.gfxWndWidth / 2, 50);
		Vector2 plotScale = new Vector2(5, 0.01f);
		float[] prunnedSpectralFlux;
		float[] beatTimes;

		const float startCounterSecs = 3;
		float timeLeftToStart = 0;

		Action pauseAction = null;

		bool finished = false;
		Texture2D bgTexture = null;

		const int scorePup = 2;
		const int scoreAsteroid = -1;
		public int score = 0;

		public GameState_Play(string musicFile)
		{
			this.musicFile = musicFile;

			camera = new Camera_FollowShip();

			skyBox = new Skybox(
				GameMultiVerse.Instance.skyBoxTextures[new Random().Next(GameMultiVerse.Instance.skyBoxTextures.Length)],
				GameMultiVerse.skyboxSize,
				GameMultiVerse.Instance.Content);

			ship = new Ship();
			ship.moveSpd = gameSpd;
			ship.Position = new Vector3(0, 0, -ship.moveSpd * startCounterSecs + shipOffsetZ);

			prunnedSpectralFlux = new float[1];
			beatTimes = new float[1];
			SongUtils.Instance.PerformBeatDetection("MusicFiles/" + musicFile, false, ref prunnedSpectralFlux, ref beatTimes);

			GenerateObstacles();

			uint lengthInMs = SongUtils.Instance.GetLengthInMs("MusicFiles/" + musicFile);
			levelLength = gameSpd * ((float)lengthInMs / 1000);

			frequency = SongUtils.Instance.GetFrequency("MusicFiles/" + musicFile);

			// kezdeti beállítás, hogy a kamera a hajót kövesse
			GameTime stateStartTime = new GameTime(new TimeSpan(0), new TimeSpan(0));
			ship.Update(stateStartTime);
			camera.Update(stateStartTime, ship.Position);

			bgTexture = new Texture2D(GameMultiVerse.Instance.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
			bgTexture.SetData(new[] { Color.White });

			pauseAction = delegate
				{
					if (MenuManager.IsEmpty())
					{
						MenuManager.Push(new MenuState_Pause());
					}
					else
					{
						MenuManager.Pop();
					}
				};
		}

		void GenerateObstacles()
		{
			pups.Clear();
			asteroids.Clear();

			Random randomizer = new Random();
			Vector3 pos;
			int r;
			for (int i = 0; i < beatTimes.Length; i++)
			{
				pos = new Vector3(randomizer.Next(-GameMultiVerse.levelWidth / 2, GameMultiVerse.levelWidth / 2),
								randomizer.Next(-GameMultiVerse.levelHeight / 2, GameMultiVerse.levelHeight / 2),
								gameSpd * (beatTimes[i] / 1000) );


				r = randomizer.Next(0, 100);

				if (r < 20)
				{
					pups.Add(new Obstacle(pos, Color.Yellow));
				}
				else
				{
					asteroids.Add(new Obstacle(pos, Color.White));
				}

				allBeatPositions.Enqueue(pos);
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (!MenuManager.IsEmpty())
			{
				SongUtils.Instance.SetPaused(true);
				return;
			}
			else
			{
				SongUtils.Instance.SetPaused(false);
			}

			if (timeLeftToStart != 0)
			{
				timeLeftToStart -= (float)gameTime.ElapsedGameTime.TotalSeconds;

				if (timeLeftToStart <= 0)
				{
					timeLeftToStart = 0;
					SongUtils.Instance.PlaySong("MusicFiles/" + musicFile);
				}
			}
			else
			{
				plotOffset.X -= ((float)gameTime.ElapsedGameTime.TotalSeconds * frequency)
					/ (SongUtils.FFTW_WINDOW_SIZE / plotScale.X);
			}

			if (ship.Position.Z > levelLength && !finished)
			{
				FadeManager.Fade(new Color(0, 0, 0, 0), new Color(0, 0, 0, 255), .5f,
					delegate { MenuManager.Pop1By1(); MenuManager.Push(new MenuState_Score(score)); });
				finished = true;
				InputManager.RemoveAction(pauseAction);
			}

			if (allBeatPositions.Count != 0)
			{
				if (ship.Position.Z >= allBeatPositions.Peek().Z)
				{
					FadeManager.Fade(new Color(255, 255, 255, 128), new Color(255, 255, 255, 0), .5f);
					allBeatPositions.Dequeue();
				}
			}

			foreach (Obstacle o in asteroids)
			{
				foreach (BoundingBox sbb in ship.boundingBoxes)
				{
					if (o.BB.Intersects(sbb))
					{
						blackList.Add(o);
						score += scoreAsteroid;
						break;
					}
				}
			}

			foreach (Obstacle o in pups)
			{
				foreach (BoundingBox sbb in ship.boundingBoxes)
				{
					if (o.BB.Intersects(sbb))
					{
						blackList.Add(o);
						score += scorePup;
						break;
					}
				}
			}

			foreach (Obstacle o in blackList)
			{
				asteroids.Remove(o);
			}
			foreach (Obstacle o in blackList)
			{
				pups.Remove(o);
			}

			ship.Update(gameTime);

			camera.Update(gameTime, ship.Position);
		}

		public override void Draw(GameTime gameTime)
		{
			skyBox.Draw(camera.V, camera.P, camera.position, GameMultiVerse.Instance.GraphicsDevice);

			ship.Draw(camera.V, camera.P);

			foreach (Obstacle o in asteroids)
			{
				o.Draw(camera.V, camera.P);
			}

			foreach (Obstacle o in pups)
			{
				o.Draw(camera.V, camera.P);
			}

			#region Edges
			float zPos = levelLength / 2;
			float zScale = levelLength / 2;
			float offset = 10;

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(GameMultiVerse.levelWidth / 2 + offset / 2,
															GameMultiVerse.levelHeight / 2 + offset / 2,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(-GameMultiVerse.levelWidth / 2 - offset / 2,
															GameMultiVerse.levelHeight / 2 + offset / 2,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(GameMultiVerse.levelWidth / 2 + offset / 2,
															-GameMultiVerse.levelHeight / 2 - offset / 2,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(-GameMultiVerse.levelWidth / 2 - offset / 2,
															-GameMultiVerse.levelHeight / 2 - offset / 2,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(GameMultiVerse.levelWidth / 2 + offset,
															0,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(-GameMultiVerse.levelWidth / 2 - offset,
															0,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(0,
															GameMultiVerse.levelHeight / 2 + offset,
															zPos),
							camera.V, camera.P, Color.Cyan);

			Utils3D.DrawCube(Matrix.CreateScale(1, 1, zScale)
								* Matrix.CreateTranslation(0,
															-GameMultiVerse.levelHeight / 2 - offset,
															zPos),
							camera.V, camera.P, Color.Cyan);
			#endregion Edges

			#region 2D
			GameMultiVerse.Instance.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			//Utils2D.DrawLine(2, Color.Red,
			//    new Vector2(GameMultiVerse.gfxWndWidth / 2, 0),
			//    new Vector2(GameMultiVerse.gfxWndWidth / 2, GameMultiVerse.gfxWndHeight));
			//Utils2D.DrawPlot(prunnedSpectralFlux, plotOffset, plotScale,
			//    GameMultiVerse.gfxWndHeight - plotOffset.Y * 2);

			if (timeLeftToStart != 0)
			{
				string counterText = timeLeftToStart.ToString();
				GameMultiVerse.Instance.spriteBatch.DrawString(GameMultiVerse.Instance.spriteFont,
																counterText,
																new Vector2(GameMultiVerse.gfxWndWidth / 2,
																	GameMultiVerse.gfxWndHeight / 2),
																Color.Red,
																0,
																GameMultiVerse.Instance.spriteFont.MeasureString(counterText) / 2,
																1,
																SpriteEffects.None,
																0);
			}
			else
			{
				string text = score.ToString();
				GameMultiVerse.Instance.spriteBatch.DrawString(GameMultiVerse.Instance.spriteFont,
									text,
									new Vector2(0, 0),
									Color.White,
									0,
									new Vector2(0, 0),
									1,
									SpriteEffects.None,
									0);

				if (finished && !MenuManager.IsEmpty())
				{
					GameMultiVerse.Instance.spriteBatch.Draw(bgTexture,
						new Rectangle(0, 0, GameMultiVerse.gfxWndWidth, GameMultiVerse.gfxWndHeight),
						Color.Black);
				}
			}

			GameMultiVerse.Instance.spriteBatch.End();

			// ezek visszaállítása szükséges a 3D részek(pl.: skybox) megfelelő(normál) megjelenítéséhez
			// mivel a 2D rajzolás viszont más beállításokat igényel
			GameMultiVerse.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
			GameMultiVerse.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
			#endregion 2D
		}

		public override void Start()
		{
			finished = false;

			timeLeftToStart = startCounterSecs;

			FadeManager.Fade(new Color(0, 0, 0, 255), new Color(0, 0, 0, 0), .5f);

			InputManager.AddAction(pauseAction, InputManager.KeyTriggerState.DOWN, Keys.Escape, Keys.Pause, Keys.Back);

			score = 0;

			MenuManager.Push(new MenuState_Pause());
		}

		public void Restart()
		{
			MenuManager.Pop1By1();

			SongUtils.Instance.ReleaseSong();

			plotOffset = new Vector2(GameMultiVerse.gfxWndWidth / 2, 50);

			ship.Position = new Vector3(0, 0, -ship.moveSpd * startCounterSecs + shipOffsetZ);

			// kezdeti beállítás, hogy a kamera a hajót kövesse
			GameTime stateStartTime = new GameTime(new TimeSpan(0), new TimeSpan(0));
			ship.Update(stateStartTime);
			camera.Update(stateStartTime, ship.Position);

			GenerateObstacles();

			Start();
		}

		public override void Exit()
		{
			MenuManager.Pop1By1();
		}
	}
}
