using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MultiVerse
{
	class Obstacle
	{
		Model unitCube = null;
		BasicEffect basicEffect = null;

		BoundingBox boundingBox;
		public BoundingBox BB
		{
			get
			{
				return boundingBox;
			}
		}

		public Obstacle(Vector3 position, Color color)
		{
			unitCube = GameMultiVerse.Instance.Content.Load<Model>("Models/cube");
			basicEffect = new BasicEffect(GameMultiVerse.Instance.GraphicsDevice);

			Matrix W = Matrix.CreateTranslation(position);
			boundingBox = Collision.UpdateBoundingBox(unitCube, W);

			basicEffect.EnableDefaultLighting();
			basicEffect.World = W;
			basicEffect.DiffuseColor = color.ToVector3();
		}

		public void Draw (Matrix V, Matrix P)
		{
			foreach (ModelMesh mesh in unitCube.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					basicEffect.View = V;
					basicEffect.Projection = P;

					part.Effect = basicEffect;
				}

				mesh.Draw();
			}
		}
	}
}
