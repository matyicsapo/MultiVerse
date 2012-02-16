using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	abstract class Camera
	{
		public Matrix V = Matrix.Identity;
		public Matrix P = Matrix.Identity;

		public Vector3 position = Vector3.Zero;

		public Camera(Vector3 pos, Vector3 lookAt, Vector3 upVector, float degFOV, float aspectRatio, float dstNearPlane, float dstFarPlane)
		{
			V = Matrix.CreateLookAt(pos, lookAt, upVector);
			P = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(degFOV), aspectRatio, dstNearPlane, dstFarPlane);

			position = pos;
		}
	}
}
