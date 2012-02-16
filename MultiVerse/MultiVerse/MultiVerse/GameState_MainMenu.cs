using Microsoft.Xna.Framework;
using Background;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace MultiVerse
{
	public class GameState_MainMenu : GameState
	{
		Skybox skyBox = null;

		Matrix V = Matrix.Identity;
		Matrix P = Matrix.Identity;

		Vector3 cameraRotation = Vector3.Zero;
		const float cameraRotationSpeed = .05f;

		public GameState_MainMenu()
		{
			skyBox = new Skybox(
				GameMultiVerse.Instance.skyBoxTextures[new Random().Next(GameMultiVerse.Instance.skyBoxTextures.Length)],
				GameMultiVerse.skyboxSize,
				GameMultiVerse.Instance.Content);

			V = Matrix.CreateLookAt(Vector3.Zero, Vector3.One, Vector3.Up);
			P = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameMultiVerse.FOV),
				GameMultiVerse.Instance.GraphicsDevice.Viewport.AspectRatio,
				0.1f, GameMultiVerse.cameraViewDistance);

			MenuManager.Push(new MenuState_Main());
		}

		public override void Update(GameTime gameTime)
		{
			cameraRotation += Vector3.One * (float)gameTime.ElapsedGameTime.TotalSeconds * cameraRotationSpeed;

			V = Matrix.CreateLookAt(Vector3.Zero,
				Vector3.Transform(Vector3.One, Matrix.CreateRotationX(cameraRotation.X) *
												Matrix.CreateRotationX(cameraRotation.Y) *
												Matrix.CreateRotationX(cameraRotation.Z)),
				Vector3.Up);
		}

		public override void Draw(GameTime gameTime)
		{
			skyBox.Draw(V, P, Vector3.Zero, GameMultiVerse.Instance.GraphicsDevice);
		}

		public override void Start()
		{
			FadeManager.Fade(new Color(0, 0, 0, 255), new Color(0, 0, 0, 0), .5f);
		}

		public override void Exit()
		{
			MenuManager.Pop1By1();
		}
	}
}
