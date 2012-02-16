using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiVerse
{
	static class Utils3D
	{
		static BasicEffect basicEffect = null;
		static Model cube = null;

		static Utils3D()
		{
			cube = GameMultiVerse.Instance.Content.Load<Model>("Models/cube");

			basicEffect = new BasicEffect(GameMultiVerse.Instance.GraphicsDevice);
		}

		public static void DrawCube(Matrix W, Matrix V, Matrix P, Color color)
		{
			foreach (ModelMesh mesh in cube.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					// fontos a sorrend !!
					// ha a forgatást a mozgatás után végeznénk el akkor is a (0, 0, 0) pont körül forgatna de mivel az már
					//	nem a középpontja ezért teljesen más eredményt kapnánk
					// ha előbb skáláznánk aztán mozgatnánk akkor a skálázás arányának megfelelően kéne változtatni
					//	a mozgatást is
					// a forgatást a skálázás után kell elvégezni valószinüleg..mármint hogy ne legyen semmi féle szétnyúlás
					//	ami valószinüleg nem kívánt

					basicEffect.EnableDefaultLighting();

					basicEffect.DiffuseColor = color.ToVector3();

					basicEffect.World = W;

					basicEffect.View = V;
					basicEffect.Projection = P;

					part.Effect = basicEffect;
				}

				mesh.Draw();
			}
		}
	}
}
