using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Text.RegularExpressions;

namespace MultiVerse
{
	class MenuState_TrackSelection : MenuState
	{
		const int maxTracks = 5;

		List<MenuEntry> menuEntries = null;

		int activeMenuEntry = 0;

		int start = 0;
		int end = 0;

		Action changeMenuActionUP = null;
		Action changeMenuActionDOWN = null;

		Action onActivateMenuEntry = null;

		Action backAction = null;

		public MenuState_TrackSelection()
		{
			menuEntries = new List<MenuEntry>();

			List<string> musicfiles = Directory.EnumerateFiles("MusicFiles").ToList<string>();

			string regexPattern = @"[*.mp3|*.flac]$";

			musicfiles = musicfiles.Where(m => Regex.IsMatch(m, regexPattern)).ToList<string>();

			MenuEntry fileEntry = null;
			foreach (string mFile in musicfiles)
			{
			    int iFileNameStart = mFile.IndexOf('\\') + 1;
			    string s = mFile.Substring(iFileNameStart, mFile.Length - iFileNameStart);

				fileEntry = new MenuEntry(s,
					delegate
					{
						FadeManager.Fade(new Color(0, 0, 0, 0), new Color(0, 0, 0, 255), .5f,
							delegate
							{
								GameMultiVerse.Instance.CurrentGameState = new GameState_Play(s);
							});
					});

				menuEntries.Add(fileEntry);
			}

			backAction = delegate { MenuManager.Pop(); };
			menuEntries.Add(new MenuEntry("Back", backAction));

			changeMenuActionDOWN = delegate
			{
				if (activeMenuEntry < menuEntries.Count - 1)
				{
					activeMenuEntry++;

					ScrollMenu();
				}
			};

			changeMenuActionUP = delegate
			{
				if (activeMenuEntry > 0)
				{
					activeMenuEntry--;

					ScrollMenu();
				}
			};

			onActivateMenuEntry = new Action(ActivateMenuEntry);

			ScrollMenu();
		}

		void ScrollMenu()
		{
			if (menuEntries.Count - 1 < maxTracks)
			{
				start = 0;
				end = menuEntries.Count - 1;

				return;
			}

			// a menük kirajzolása az active előttől maximum a MAX felével kezdődnek
			start = activeMenuEntry - (int)(maxTracks / 2);

			// ha netán negatív volna
			start = start < 0 ? 0 : start;

			// 'start'-tól 'maxTracks' darabot rajzolunk ki
			end = start + maxTracks;

			// ha az 'end' "túlmutatna" a végén akkor lekorlátozzuk
			//	azért '-1' mert a tracklista után van még egy back menüpont a visszalépéshez
			end = end > menuEntries.Count - 1 ? menuEntries.Count - 1 : end;

			if (end - start < maxTracks)
			{
				start -= maxTracks - (end - start);
			}
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

			Vector2 offset = new Vector2(0, -GameMultiVerse.Instance.Window.ClientBounds.Height / 2 + 200);

			if (menuEntries.Count > 1)
			{
				for (int i = start; i < end; i++)
				{
					menuEntries[i].Draw(offset, i == activeMenuEntry);
					offset.Y += GameMultiVerse.menuSpacingV;
				}
			}

			offset.Y += GameMultiVerse.menuSpacingV;
			menuEntries[menuEntries.Count - 1].Draw(offset, menuEntries.Count - 1 == activeMenuEntry);

			GameMultiVerse.Instance.spriteBatch.End();

			// ezek visszaállítása szükséges a 3D részek(pl.: skybox) megfelelő(normál) megjelenítéséhez
			// mivel a 2D rajzolás viszont más beállításokat igényel
			GameMultiVerse.Instance.GraphicsDevice.BlendState = BlendState.Opaque;
			GameMultiVerse.Instance.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
		}
	}
}
