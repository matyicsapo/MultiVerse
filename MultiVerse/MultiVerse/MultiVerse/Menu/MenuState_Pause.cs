using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	public class MenuState_Pause : MenuState
	{
		List<MenuEntry> menuEntries = null;

		int activeMenuEntry = 0;

		Action changeMenuActionUP = null;
		Action changeMenuActionDOWN = null;

		Action onActivateMenuEntry = null;

		public MenuState_Pause()
		{
			menuEntries = new List<MenuEntry>();

			menuEntries.Add(new MenuEntry("Resume", delegate { MenuManager.Pop(); }));

			menuEntries.Add(new MenuEntry("Restart Track",
				delegate
				{
					FadeManager.Fade(new Color(0, 0, 0, 0), new Color(0, 0, 0, 255), .5f,
							delegate
							{
								(GameMultiVerse.Instance.CurrentGameState as GameState_Play).Restart();
							});
				}));

			menuEntries.Add(new MenuEntry("Different Track", delegate { MenuManager.Push(new MenuState_TrackSelection()); }));

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

			Vector2 offset = new Vector2(0, -200);
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
