using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace MultiVerse
{
	public static class InputManager
	{
		[FlagsAttribute]
		public enum KeyTriggerState { DOWN = 1, UP = 2, HELD = 4 };

		static KeyboardState lastKeyboardState = new KeyboardState();
		static KeyboardState currKeyboardState = new KeyboardState();

		static Dictionary<Action, Dictionary<Keys, KeyTriggerState>> keyboardActions =
			new Dictionary<Action, Dictionary<Keys, KeyTriggerState>>();

		public static void Update()
		{
			lastKeyboardState = currKeyboardState;

			currKeyboardState = Keyboard.GetState();

			bool trigger;

			foreach (KeyValuePair<Action, Dictionary<Keys, KeyTriggerState>> actionEntry in keyboardActions)
			{
				trigger = false;

				foreach (KeyValuePair<Keys, KeyTriggerState> triggerEntry in actionEntry.Value)
				{
					// nem kell 'else' !!

					if (triggerEntry.Value.HasFlag(KeyTriggerState.HELD))
					{
						if (currKeyboardState.IsKeyDown(triggerEntry.Key))
						{
							trigger = true;
							break;
						}
					}
					
					if (triggerEntry.Value.HasFlag(KeyTriggerState.DOWN))
					{
						if (!lastKeyboardState.IsKeyDown(triggerEntry.Key) && currKeyboardState.IsKeyDown(triggerEntry.Key))
						{
							trigger = true;
							break;
						}
					}

					if (triggerEntry.Value.HasFlag(KeyTriggerState.UP))
					{
						if (lastKeyboardState.IsKeyDown(triggerEntry.Key) && !currKeyboardState.IsKeyDown(triggerEntry.Key))
						{
							trigger = true;
							break;
						}
					}
				}

				if (trigger)
				{
					actionEntry.Key();
				}
			}
		}

		public static void AddAction(Action action, KeyTriggerState triggerOn, params Keys[] keyboardKeys)
		{
			Dictionary<Keys, KeyTriggerState> keyboardTriggers = null;

			// ha az action már benn van, akkor lekérjük az hozzátartozó billentyű + state dictionaryt
			if (keyboardActions.TryGetValue(action, out keyboardTriggers))
			{
				// az összes beadott billentyűn végigmegyünk
				foreach (Keys keyboardKey in keyboardKeys)
				{
					if (keyboardTriggers.ContainsKey(keyboardKey))
					{
						// ha eddig is bennvolt a key akkor a state-jéhez hozzá bitwise OR-ozzuk a 'triggerOn'-ban megadottat
						keyboardTriggers[keyboardKey] |= triggerOn;
					}
					else
					{
						// ha ez a billentyű még egyáltalán nem volt benn akkor a megadott 'triggerOn' paraméterrel
						// hozzáadjuk ennek az action-nek dictionaryjéhez
						keyboardTriggers.Add(keyboardKey, triggerOn);
					}
				}
			}
			else
			{
				keyboardTriggers = new Dictionary<Keys, KeyTriggerState>();

				foreach (Keys keyboardKey in keyboardKeys)
				{
					keyboardTriggers.Add(keyboardKey, triggerOn);
				}

				keyboardActions.Add(action, keyboardTriggers);
			}
		}

		public static void ClearAllActions()
		{
			keyboardActions.Clear();
		}

		public static void RemoveAction(Action action)
		{
			keyboardActions.Remove(action);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="action"></param>
		/// <param name="keyboardKey">Legalább egy billenytű gombot meg kell adni.</param>
		/// <param name="keyboardKeys">Opcionálisan megadható további billentyűk.</param>
		public static void RemoveKey(Action action, Keys keyboardKey, params Keys[] keyboardKeys)
		{
			Dictionary<Keys, KeyTriggerState> keyboardTriggers = null;

			if (keyboardActions.TryGetValue(action, out keyboardTriggers))
			{
				keyboardTriggers.Remove(keyboardKey);

				foreach (Keys kKey in keyboardKeys)
				{
					keyboardTriggers.Remove(kKey);
				}
			}
		}

		public static void RemoveKeyTriggerState(Action action, Keys keyboardKey, KeyTriggerState dontTriggerOn)
		{
			Dictionary<Keys, KeyTriggerState> keyboardTriggers = null;

			if (keyboardActions.TryGetValue(action, out keyboardTriggers))
			{
				if (keyboardTriggers.ContainsKey(keyboardKey))
				{
					// bitwise & PLUS bitwise complement EQUALS remove flag
					keyboardTriggers[keyboardKey] &= ~dontTriggerOn;
				}
			}
		}
	}
}
