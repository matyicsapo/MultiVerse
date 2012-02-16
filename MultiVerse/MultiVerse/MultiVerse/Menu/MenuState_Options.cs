using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Input;

namespace MultiVerse
{
	public class MenuState_Options : MenuState
	{
		List<MenuEntry> menuEntries = null;

		int activeMenuEntry = 0;

		Action changeMenuActionUP = null;
		Action changeMenuActionDOWN = null;

		Action onActivateMenuEntry = null;

		Action backAction = null;

		public MenuState_Options()
		{
			menuEntries = new List<MenuEntry>();
			menuEntries.Add(new MenuEntry("Inverse Y: " + GameMultiVerse.Instance.inverseY.ToString(), delegate { GameMultiVerse.Instance.inverseY = !GameMultiVerse.Instance.inverseY;
																													menuEntries[activeMenuEntry].text = "Inverse Y: " + GameMultiVerse.Instance.inverseY.ToString();
																												}));
			//menuEntries.Add(new MenuEntry("Show Spectrum: " + GameMultiVerse.Instance.showSpectrum.ToString(), delegate
			//{
			//    GameMultiVerse.Instance.showSpectrum = !GameMultiVerse.Instance.showSpectrum;
			//    menuEntries[activeMenuEntry].text = "Show Spectrum: " + GameMultiVerse.Instance.showSpectrum.ToString();
			//}));
			//menuEntries.Add(new MenuEntry("Volume:", delegate {}));

			backAction = delegate { MenuManager.Pop(); };
			menuEntries.Add(new MenuEntry("Back", backAction));

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

			InputManager.AddAction(backAction, InputManager.KeyTriggerState.DOWN, Keys.Back, Keys.Escape);
		}

		public override void Exit()
		{
			InputManager.RemoveAction(changeMenuActionDOWN);
			InputManager.RemoveAction(changeMenuActionUP);

			InputManager.RemoveAction(onActivateMenuEntry);

			InputManager.RemoveAction(backAction);
		}

		public override void Draw(GameTime gameTime)
		{
			GameMultiVerse.Instance.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);

			Vector2 offset = new Vector2(0, -100);
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
