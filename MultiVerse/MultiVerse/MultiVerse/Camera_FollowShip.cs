using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	class Camera_FollowShip : Camera
	{
		Vector3 offset = new Vector3(0, 15, -30);

		public Camera_FollowShip() : base(Vector3.Zero, Vector3.Zero + Vector3.UnitZ * 1000, Vector3.Up, GameMultiVerse.FOV,
			GameMultiVerse.Instance.GraphicsDevice.Viewport.AspectRatio, .1f, GameMultiVerse.cameraViewDistance)
		{
			position = offset;
		}

		public void Update(GameTime gameTime, Vector3 target)
		{
			position = target + offset;

			float d = .75f;
			if (position.X < (-GameMultiVerse.levelWidth / 2) * d)
			{
				position.X = (-GameMultiVerse.levelWidth / 2) * d;
			}
			else if (position.X > (GameMultiVerse.levelWidth / 2) * d)
			{
				position.X = (GameMultiVerse.levelWidth / 2) * d;
			}
			if (position.Y < (-GameMultiVerse.levelHeight / 2) * d)
			{
				position.Y = (-GameMultiVerse.levelHeight / 2) * d;
			}
			else if (position.Y > (GameMultiVerse.levelHeight / 2) * d)
			{
				position.Y = (GameMultiVerse.levelHeight / 2) * d;
			}

			V = Matrix.CreateLookAt(position,
				(position + Vector3.UnitZ * 1000),
				Vector3.Up);
			P = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(GameMultiVerse.FOV),
				GameMultiVerse.Instance.GraphicsDevice.Viewport.AspectRatio,
				0.1f, GameMultiVerse.cameraViewDistance);
		}
	}
}
