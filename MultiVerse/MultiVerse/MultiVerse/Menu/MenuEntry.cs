using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiVerse
{
	class MenuEntry
	{
		public readonly Action action = null;

		public string text;

		public MenuEntry(string text, Action action)
		{
			this.text = text;
			this.action = action;
		}

		public void Draw(Vector2 offset, bool active)
		{
			GameMultiVerse.Instance.spriteBatch.DrawString(GameMultiVerse.Instance.spriteFont,
									text,
									new Vector2(GameMultiVerse.Instance.Window.ClientBounds.Width / 2f,
										GameMultiVerse.Instance.Window.ClientBounds.Height / 2f)
										+ offset,
									active ? Color.Yellow : Color.White,
									0,
									GameMultiVerse.Instance.spriteFont.MeasureString(text) / 2,
									1,
									SpriteEffects.None,
									0);
		}
	}
}
