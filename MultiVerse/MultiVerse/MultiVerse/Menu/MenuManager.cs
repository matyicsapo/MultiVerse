using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiVerse
{
	public static class MenuManager
	{
		static Stack<MenuState> menuStateStack = new Stack<MenuState>();

		static MenuState pushed = null;

		static bool popped = false;

		static bool pop1by1 = false;

		public static bool IsEmpty()
		{
			return menuStateStack.Count == 0;
		}

		public static void Push(MenuState menu)
		{
			pushed = menu;
		}

		public static void Pop()
		{
			if (menuStateStack.Count != 0)
			{
				popped = true;
			}
		}

		/// <summary>
		/// Remove all MenuStates calling each's Exit() method 1 by 1 from the top.
		/// </summary>
		public static void Pop1By1()
		{
			pop1by1 = true;
		}

		public static void Update(GameTime gameTime)
		{
			if (menuStateStack.Count != 0)
			{
				menuStateStack.Peek().Update(gameTime);
			}
		}

		public static void Draw(GameTime gameTime)
		{
			if (menuStateStack.Count != 0)
			{
				menuStateStack.Peek().Draw(gameTime);
			}

			if (pop1by1)
			{
				while (menuStateStack.Count != 0)
				{
					menuStateStack.Pop().Exit();
				}

				pop1by1 = false;
			}
			else if (popped)
			{
				menuStateStack.Pop().Exit();
				popped = false;

				if (menuStateStack.Count != 0)
					menuStateStack.Peek().Start();
			}

			if (pushed != null)
			{
				if (menuStateStack.Count != 0)
					menuStateStack.Peek().Exit();

				menuStateStack.Push(pushed);
				pushed.Start();
				pushed = null;
			}
		}
	}
}
