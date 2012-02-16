using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	class Ship
	{
		Model shipModel;

		float yaw = 0;
		const float yawSpd = 100;
		const float yawLimit = 15;

		float pitch = 0;
		const float pitchSpd = 100;
		const float pitchLimit = 30;

		float roll = 0;
		const float rollSpd = 100;
		const float rollLimit = 10;

		public float moveSpd = 1;
		public float strafeSpd = .3f;

		Vector3 position = new Vector3(0, 0, 0);
		public Vector3 Position
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		Matrix R = Matrix.Identity;
		Matrix W = Matrix.Identity;

		public readonly BoundingBox[] boundingBoxes = new BoundingBox[4];

		Model bb_unitCube;

		public Ship ()
		{
			shipModel = GameMultiVerse.Instance.Content.Load<Model>("Models/p1_wedge");

			bb_unitCube = GameMultiVerse.Instance.Content.Load<Model>("Models/cube");
		}

		public void Update(GameTime gameTime)
		{
			KeyboardState ks = Keyboard.GetState();

			if (GameMultiVerse.Instance.inverseY ? ks.IsKeyDown(Keys.W) : ks.IsKeyDown(Keys.S) ||
				GameMultiVerse.Instance.inverseY ? ks.IsKeyDown(Keys.Up) : ks.IsKeyDown(Keys.Down))
			{
				pitch -= pitchSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (pitch < -pitchLimit)
					pitch = -pitchLimit;
			}
			else if (GameMultiVerse.Instance.inverseY ? ks.IsKeyDown(Keys.S) : ks.IsKeyDown(Keys.W) |
					GameMultiVerse.Instance.inverseY ? ks.IsKeyDown(Keys.Down) : ks.IsKeyDown(Keys.Up))
			{
				pitch += pitchSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (pitch > pitchLimit)
					pitch = pitchLimit;
			}

			#region Yaw & Roll
			if (ks.IsKeyDown(Keys.A) || ks.IsKeyDown(Keys.Left))
			{
				position.X += strafeSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
			    if (position.X > GameMultiVerse.levelWidth / 2)
			    {
			        position.X = GameMultiVerse.levelWidth / 2;
			    }

			    yaw += yawSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
			    if (yaw > yawLimit)
			        yaw = yawLimit;

				roll += rollSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (roll > rollLimit)
					roll = rollLimit;
			}
			else if (ks.IsKeyDown(Keys.D) || ks.IsKeyDown(Keys.Right))
			{
				position.X -= strafeSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (position.X < -GameMultiVerse.levelWidth / 2)
				{
					position.X = -GameMultiVerse.levelWidth / 2;
				}

				yaw -= yawSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (yaw < -yawLimit)
					yaw = -yawLimit;

				roll -= rollSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (roll < -rollLimit)
					roll = -rollLimit;
			}
			#endregion Yaw & Roll

			Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(yaw), MathHelper.ToRadians(pitch), 0, out R);

			Vector3 motion = Vector3.Zero;
			motion.Z = moveSpd * (float)gameTime.ElapsedGameTime.TotalSeconds;
			position += motion;

			position.Y -= Vector3.Transform(motion, R).Y;
			if (position.Y < -GameMultiVerse.levelHeight / 2)
			{
				position.Y = -GameMultiVerse.levelHeight / 2;
			}
			else if (position.Y > GameMultiVerse.levelHeight / 2)
			{
				position.Y = GameMultiVerse.levelHeight / 2;
			}

			position.X += Vector3.Transform(motion, R).X;
			if (position.X < -GameMultiVerse.levelWidth / 2)
			{
				position.X = -GameMultiVerse.levelWidth/ 2;
			}
			else if (position.X > GameMultiVerse.levelWidth / 2)
			{
				position.X = GameMultiVerse.levelWidth/ 2;
			}

			W = R
				* Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(180), 0, MathHelper.ToRadians(roll))
				* Matrix.CreateTranslation(position);

			#region Updating bounding shapes
			// left wing
			Vector3 bb_position = new Vector3(-2.2f, -0.8f, 0);
			Vector3 bb_rotation = new Vector3(0, 50, 15);
			Vector3 bb_scale = new Vector3(3, .5f, 1);
			Matrix bb_W = Matrix.Identity;
			bb_W *= Matrix.CreateScale(bb_scale) *
						Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
						Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
						Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
					* Matrix.CreateTranslation(bb_position)
				;
			bb_W *= W;
			boundingBoxes[0] = Collision.UpdateBoundingBox(bb_unitCube, bb_W);

			// right wing
			bb_position = new Vector3(2.2f, -0.8f, 0);
			bb_rotation = new Vector3(0, -50, -15);
			bb_scale = new Vector3(3, .5f, 1);
			bb_W = Matrix.Identity;
			bb_W *= Matrix.CreateScale(bb_scale) *
						Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
						Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
						Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
					* Matrix.CreateTranslation(bb_position)
				;
			bb_W *= W;
			boundingBoxes[1] = Collision.UpdateBoundingBox(bb_unitCube, bb_W);

			// body
			bb_position = new Vector3(0, 0, -1);
			bb_rotation = new Vector3(0, 0, 0);
			bb_scale = new Vector3(1, 1, 4);
			bb_W = Matrix.Identity;
			bb_W *= Matrix.CreateScale(bb_scale) *
						Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
						Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
						Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
					* Matrix.CreateTranslation(bb_position)
				;
			bb_W *= W;
			boundingBoxes[2] = Collision.UpdateBoundingBox(bb_unitCube, bb_W);

			// tail
			bb_position = new Vector3(0, 2.25f, 5.2f);
			bb_rotation = new Vector3(0, 0, 0);
			bb_scale = new Vector3(1f, 1f, 5.5f);
			bb_W = Matrix.Identity;
			bb_W *= Matrix.CreateScale(bb_scale) *
						Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
						Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
						Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
					* Matrix.CreateTranslation(bb_position)
				;
			bb_W *= W;
			boundingBoxes[3] = Collision.UpdateBoundingBox(bb_unitCube, bb_W);
			#endregion
		}

		public void Draw(Matrix V, Matrix P)
		{
			//#region DEBUG bb
			//RasterizerState originalRasterizerState = GameMultiVerse.Instance.GraphicsDevice.RasterizerState;

			//GameMultiVerse.Instance.GraphicsDevice.RasterizerState = new RasterizerState()
			//{
			//    FillMode = FillMode.WireFrame,
			//    CullMode = CullMode.None,
			//};

			//// left wing
			//Color bb_color = Color.Magenta;
			//Vector3 bb_position = new Vector3(-2.2f, -0.8f, 0);
			//Vector3 bb_rotation = new Vector3(0, 50, 15);
			//Vector3 bb_scale = new Vector3(3, .5f, 1);
			//Matrix bb_W = Matrix.Identity;
			//bb_W *= Matrix.CreateScale(bb_scale) *
			//            Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
			//            Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
			//            Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
			//        * Matrix.CreateTranslation(bb_position)
			//    ;
			//bb_W *= W;
			//Utils3D.DrawCube(bb_W, V, P, bb_color);

			//// right wing
			//bb_position = new Vector3(2.2f, -0.8f, 0);
			//bb_rotation = new Vector3(0, -50, -15);
			//bb_scale = new Vector3(3, .5f, 1);
			//bb_W = Matrix.Identity;
			//bb_W *= Matrix.CreateScale(bb_scale) *
			//            Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
			//            Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
			//            Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
			//        * Matrix.CreateTranslation(bb_position)
			//    ;
			//bb_W *= W;
			//Utils3D.DrawCube(bb_W, V, P, bb_color);

			//// body
			//bb_position = new Vector3(0, 0, -1);
			//bb_rotation = new Vector3(0, 0, 0);
			//bb_scale = new Vector3(1, 1, 4);
			//bb_W = Matrix.Identity;
			//bb_W *= Matrix.CreateScale(bb_scale) *
			//            Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
			//            Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
			//            Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
			//        * Matrix.CreateTranslation(bb_position)
			//    ;
			//bb_W *= W;
			//Utils3D.DrawCube(bb_W, V, P, bb_color);

			//// tail
			//bb_position = new Vector3(0, 2.25f, 5.2f);
			//bb_rotation = new Vector3(0, 0, 0);
			//bb_scale = new Vector3(1f, 1f, 5.5f);
			//bb_W = Matrix.Identity;
			//bb_W *= Matrix.CreateScale(bb_scale) *
			//            Matrix.CreateRotationX(MathHelper.ToRadians(bb_rotation.X)) *
			//            Matrix.CreateRotationY(MathHelper.ToRadians(bb_rotation.Y)) *
			//            Matrix.CreateRotationZ(MathHelper.ToRadians(bb_rotation.Z))
			//        * Matrix.CreateTranslation(bb_position)
			//    ;
			//bb_W *= W;
			//Utils3D.DrawCube(bb_W, V, P, bb_color);

			//GameMultiVerse.Instance.GraphicsDevice.RasterizerState = originalRasterizerState;
			//#endregion DEBUG bb

			#region ship
			foreach (ModelMesh mesh in shipModel.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.EnableDefaultLighting();

					effect.World = W;

					effect.View = V;
					effect.Projection = P;
				}

				mesh.Draw();
			}
			#endregion ship
		}
	}
}
