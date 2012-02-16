using Microsoft.Xna.Framework;

namespace MultiVerse
{
	public abstract class State
	{
		public virtual void Update(GameTime gameTime) { }

		public virtual void Draw(GameTime gameTime) { }

		public virtual void Start() { }

		public virtual void Exit() { }
	}
}
