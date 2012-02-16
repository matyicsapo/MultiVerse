using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	public class MenuState_Main : MenuState
	{
		List<MenuEntry> menuEntries = null;

		int activeMenuEntry = 0;

		Action changeMenuActionUP = null;
		Action changeMenuActionDOWN = null;

		Action onActivateMenuEntry = null;

		public MenuState_Main()
		{
			menuEntries = new List<MenuEntry>();
			menuEntries.Add(new MenuEntry("Play", delegate { MenuManager.Push(new MenuState_TrackSelection()); }));
			menuEntries.Add(new MenuEntry("Options", delegate { MenuManager.Push(new MenuState_Options()); }));
			menuEntries.Add(new MenuEntry("Exit Game", delegate { GameMultiVerse.Instance.Exit(); }));

			changeMenuActionDOWN = delegate
			{
				if (activeMenuEntry < menuEntries.Count - 1)
					activeMenuEntry++;
			};

			changeMenuActionUP = delegate
			{
				if (activeMenuEntry > 0)
					activeMenuEntry--;
			};

			onActivateMenuEntry = new Action(ActivateMenuEntry);
		}

		void ActivateMenuEntry()
		{
			menuEntries[activeMenuEntry].action();
		}

		public override void Start()
		{
			InputManager.AddAction(changeMenuActionDOWN,
				InputManager.KeyTriggerState.DOWN,
				Keys.Down, Keys.S);
			InputManager.AddAction(changeMenuActionUP,
				InputManager.KeyTriggerState.UP,
				Keys.Up, Keys.W);

			InputManager.AddAction(onActivateMenuEntry,
				InputManager.KeyTriggerState.DOWN,
				Keys.Enter, Keys.Space);
		}

		public override void Exit()
		{
			InputManager.RemoveAction(changeMenuActionDOWN);
			InputManager.RemoveAction(changeMenuActionUP);

			InputManager.RemoveAction(onActivateMenuEntry);
		}

		public override void Draw(GameTime gameTime)
		{
			GameMultiVerse.Instance.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			string text = "MultiVerse";
			GameMultiVerse.Instance.spriteBatch.DrawString(GameMultiVerse.Instance.spriteFont,
															text,
															new Vector2(GameMultiVerse.gfxWndWidth / 2,
																GameMultiVerse.gfxWndHeight / 2 - 250),
															Color.Red,
															0,
															GameMultiVerse.Instance.spriteFont.MeasureString(text) / 2,
															2,
															SpriteEffects.None,
															0);
			text = "(alpha)";
			GameMultiVerse.Instance.spriteBatch.DrawString(GameMultiVerse.Instance.spriteFont,
															text,
															new Vector2(GameMultiVerse.gfxWndWidth / 2,
																GameMultiVerse.gfxWndHeight / 2 - 175),
															Color.Green,
															0,
															GameMultiVerse.Instance.spriteFont.MeasureString(text) / 2,
															1,
															SpriteEffects.None,
															0);


			Vector2 offset = new Vector2(0, -50);
			for (int i = 0; i < menuEntries.Count; i++)
			{
				menuEntries[i].Draw(offset, i == activeMenuEntry);
				offset.Y += GameMultiVerse.menuSpacingV;
			}

			GameMultiVerse.Instance.spriteBatch.End();

			// ezek visszaállítása szükséges a 3D részek(pl.: skybox) megfelelő(normál) megjelenítéséhez
			// mivel a 2D rajzolás viszont más beállításokat igényel
			GameMultiVerse.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
			GameMultiVerse.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}
	}
}
