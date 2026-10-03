using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API {

	/// <summary>
	/// A menu for choosing between items in the console.
	/// </summary>
	/// <param name="loops">Should the menu loop?</param>
	/// <param name="numbered">Should the menu be numbered?</param>
	/// <param name="clearOnRefresh">Should the menu be cleared on refresh?</param>
	public class ConsoleSelectMenu(bool loops, bool numbered, bool clearOnRefresh) {

		public const int PulseRate = 62; // milliseconds between key checks
		public const int MaxPulses = 15; // maximum number of heartbeats before refreshing the menu

		static readonly string[] QuitPhrases = ["exit", "quit", "continue", "break", "back"];


		public bool ClearConsole { get; private set; } = clearOnRefresh;
		public bool Loops { get; private set; } = loops;
		public bool Numbered { get; private set; } = numbered;
		private bool MenuOpen = false;

		public int Selected { get; private set; } = -1;

		public string PreChoiceText { get; private set; } = "";
		public string PostChoiceText { get; private set; } = "";

		public int ChoiceCount => Choices.Count;
		private readonly List<ConsoleMenuItem> Choices = [];


		public event Action<ConsoleSelectMenu>? OnMenuOpen;
		public event Action<ConsoleSelectMenu>? OnDrawMenu;
		public event Action<ConsoleSelectMenu>? OnChoicesDisplayed;
		public event Action<ConsoleSelectMenu, int>? OnChoiceMade;
		public event Action<ConsoleSelectMenu>? OnMenuClosed;


		private int _ExitChoice = -1;
		public int ExitChoice {
			get => _ExitChoice;
			private set {
				_ExitChoice = Math.Clamp(value, -1, Choices.Count - 1);
			}
		}

		private int _YOffset = 0;
		public int YOffset {
			get => _YOffset;
			private set => _YOffset = Math.Clamp(value, 0, Console.WindowHeight - 1);
		}


		public ConsoleMenuItem this[int index] => Choices[index];


		/// <summary>
		/// Adds a choice to the menu.
		/// </summary>
		/// <param name="item">The item to be added to the menu.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddChoice(ConsoleMenuItem item) {
			// Only change the menu if it's not open.
			if (MenuOpen == false) {
				Choices.Add(item);
				if (QuitPhrases.Contains(item.Text.ToLower()) == true) {
					ExitChoice = Choices.IndexOf(item);
				}
			}
			return this;
		}

		/// <summary>
		/// Removes an item from the menu.
		/// </summary>
		/// <param name="item">The item to remove from the menu.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveChoice(ConsoleMenuItem item) {
			// Only change the menu if it's not open.
			if (MenuOpen == false) {
				int ndx = Choices.IndexOf(item);
				if (ndx != -1) {
					Choices.Remove(item);
					if (ndx == ExitChoice) {
						ExitChoice = -1; // reset exit choice if it was removed
						// get the last choice that is a quit phrase
						Choices.For((c, i) => {
							if (QuitPhrases.Contains(c.Text.ToLower()) == true) {
								ExitChoice = i; // set the quit phrase as the new exit choice
							}
						});
					}
				}
			}
			return this;
		}

		/// <summary>
		/// Displays the menu and allows the user to make a choice.
		/// </summary>
		/// <returns>The choice the user made or an error code: 
		/// -1 = menu error, 
		/// -2 = no choices, 
		/// -3 = Loops but has no exit option.
		/// -4 = Menu already open.</returns>
		public int GetChoice() {
			if (MenuOpen == false) {
				MenuOpen = true;
				OnMenuOpen?.Invoke(this);

				if (Choices.Count >= 1) {
					if (Loops == false || (ExitChoice < 0 || ExitChoice >= Choices.Count) == false) {
						Selected = 0; // set the selected index to the first choice

						int heartbeat;
						bool chosen;
						ConsoleKeyInfo key;
						bool ogVisible = Console.CursorVisible;
						Console.CursorVisible = false;

						try {
							do {
								heartbeat = 0;
								chosen = false;

								DisplayMenu();
								while (Console.KeyAvailable == false && heartbeat < MaxPulses) {
									Thread.Sleep(PulseRate);
									heartbeat += 1;
								}
								if (heartbeat < MaxPulses) {
									key = Console.ReadKey(true);
									if (key.Key == ConsoleKey.UpArrow) {
										Selected = Math.Max(0, Selected - 1);
									}
									if (key.Key == ConsoleKey.DownArrow) {
										Selected = Math.Min(Choices.Count - 1, Selected + 1);
									}
									if (key.Key != ConsoleKey.UpArrow && key.Key != ConsoleKey.DownArrow) {
										Choices[Selected].ProcessKey(key);
									}
									if (key.Key == ConsoleKey.Enter) {
										chosen = true;
										Choices[Selected].OnSelect();
										OnChoiceMade?.Invoke(this, Selected);
									}
								}

							} while (chosen == false || (Loops == true && Selected != ExitChoice));
						}
						catch (Exception ex) {
							Console.WriteLine("An error occurred while processing the menu: " + ex.Message);
							Selected = -1; // reset selected index on error
						}
						Console.CursorVisible = ogVisible;
						OnMenuClosed?.Invoke(this);
						MenuOpen = false;
						return Selected;
					}
					MenuOpen = false;
					return -3; // Loops and no exit choice.
				}
				MenuOpen = false;
				return -2; // No choices.
			}
			return -4; // Menu already open.
		}

		/// <summary>
		/// Displays the menu.
		/// </summary>
		private void DisplayMenu() {
			// clear the console if needed
			if (ClearConsole == true) {
				Console.Clear();
			}

			OnDrawMenu?.Invoke(this);

			// Display the top of the menu text.
			if (string.IsNullOrEmpty(PreChoiceText) == false) {
				Console.WriteLine(PreChoiceText);
			}

			// Display the choices in the menu.
			Choices.For((c, i) => DisplayMenuItem(c, i));

			// Display the bottom of the menu text.
			if (string.IsNullOrEmpty(PostChoiceText) == false) {
				Console.WriteLine(PostChoiceText);
			}

			OnChoicesDisplayed?.Invoke(this);
		}

		/// <summary>
		/// Displays a menu item.
		/// </summary>
		/// <param name="choice">The item to display.</param>
		/// <param name="ndx">The index the item is at within the menu.</param>
		private void DisplayMenuItem(ConsoleMenuItem choice, int ndx) {
			ExceptionDispatchInfo? ex = null;
			// Save the original console colors
			(ConsoleColor ogText, ConsoleColor ogBack) =
				(Console.ForegroundColor, Console.BackgroundColor);
			try {
				// Set the colors based on whether this choice is selected or not
				(Console.BackgroundColor, Console.ForegroundColor) = Selected != ndx ?
					(choice.BackColor, choice.TextColor) :
					(choice.TextColor, choice.BackColor);

				Console.WriteLine((Numbered == true ?
					(ndx + 1) + ": " : "")
					+ choice.Text);
			}
			catch (Exception e) {
				Console.WriteLine("An error occurred while displaying the menu item.");
				ex = ExceptionDispatchInfo.Capture(e);
			}
			// Reset the console colors to the original values
			(Console.ForegroundColor, Console.BackgroundColor) = (ogText, ogBack);
			// If an exception occurred, throw it and stop processing the menu
			if (ex != null) {
				ex.Throw();
			}
		}

		/// <summary>
		/// Subscribes a method to the OnMenuOpen event.
		/// This method allows chaining multiple subscriptions in a row on a single line.
		/// </summary>
		/// <param name="action">The method to subscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddOnMenuOpenAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnMenuOpen += action;
			}
			return this;
		}

		/// <summary>
		/// Unsubscribes a method from the OnMenuOpen event.
		/// This method allows chaining multiple unsubscribes in a row on a single line.
		/// </summary>
		/// <param name="action">The method to unsubscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveOnMenuOpenAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnMenuOpen -= action;
			}
			return this;
		}

		/// <summary>
		/// Subscribes a method to the OnDrawMenu event.
		/// This method allows chaining multiple subscriptions in a row on a single line.
		/// </summary>
		/// <param name="action">The method to subscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddOnDrawMenuAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnDrawMenu += action;
			}
			return this;
		}

		/// <summary>
		/// Unsubscribes a method from the OnDrawMenu event.
		/// This method allows chaining multiple unsubscribes in a row on a single line.
		/// </summary>
		/// <param name="action">The method to unsubscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveOnDrawMenuAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnDrawMenu -= action;
			}
			return this;
		}

		/// <summary>
		/// Subscribes a method to the OnChoicesDisplayed event.
		/// This method allows chaining multiple subscriptions in a row on a single line.
		/// </summary>
		/// <param name="action">The method to subscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddOnChoicesDisplayedAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnChoicesDisplayed += action;
			}
			return this;
		}

		/// <summary>
		/// Unsubscribes a method from the OnChoicesDisplayed event.
		/// This method allows chaining multiple unsubscribes in a row on a single line.
		/// </summary>
		/// <param name="action">The method to unsubscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveOnChoicesDisplayedAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnChoicesDisplayed -= action;
			}
			return this;
		}

		/// <summary>
		/// Subscribes a method to the OnChoiceMade event.
		/// This method allows chaining multiple subscriptions in a row on a single line.
		/// </summary>
		/// <param name="action">The method to subscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddOnChoiceMadeAction(Action<ConsoleSelectMenu, int>? action) {
			if (action != null) {
				OnChoiceMade += action;
			}
			return this;
		}

		/// <summary>
		/// Unsubscribes a method from the OnChoiceMade event.
		/// This method allows chaining multiple unsubscribes in a row on a single line.
		/// </summary>
		/// <param name="action">The method to unsubscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveOnChoiceMadeAction(Action<ConsoleSelectMenu, int>? action) {
			if (action != null) {
				OnChoiceMade -= action;
			}
			return this;
		}

		/// <summary>
		/// Subscribes a method to the OnMenuClosed event.
		/// This method allows chaining multiple subscriptions in a row on a single line.
		/// </summary>
		/// <param name="action">The method to subscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu AddOnMenuClosedAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnMenuClosed += action;
			}
			return this;
		}

		/// <summary>
		/// Unsubscribes a method from the OnMenuClosed event.
		/// This method allows chaining multiple unsubscribes in a row on a single line.
		/// </summary>
		/// <param name="action">The method to unsubscribe.</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu RemoveOnMenuClosedAction(Action<ConsoleSelectMenu>? action) {
			if (action != null) {
				OnMenuClosed -= action;
			}
			return this;
		}

		/// <summary>
		/// Sets ClearConsole and returns the menu.
		/// </summary>
		/// <param name="clear">Should the menu clear?</param>
		/// <returns>This menu.</returns>
		public ConsoleSelectMenu SetClearConsole(bool clear) {
			ClearConsole = clear;
			return this;
		}


		/// <summary>
		/// Sets Loops and returns the menu.
		/// </summary>
		/// <param name="loops">Should the menu loop?</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetLoops(bool loops) {
			Loops = loops;
			return this;
		}

		/// <summary>
		/// Sets Numbered and returns the menu.
		/// </summary>
		/// <param name="numbered">Should the menu items be numbered?</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetNumbered(bool numbered) {
			Numbered = numbered;
			return this;
		}

		/// <summary>
		/// Sets ExitChoice and returns the menu.
		/// </summary>
		/// <param name="choice">The index of the exit choice.</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetExitChoice(int choice) {
			ExitChoice = choice;
			return this;
		}

		/// <summary>
		/// Sets PreChoiceText and returns the menu.
		/// </summary>
		/// <param name="text">The text to display before all choices.</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetPreChoiceText(string text) {
			PreChoiceText = text;
			return this;
		}

		/// <summary>
		/// Sets PostChoiceText and returns the menu.
		/// </summary>
		/// <param name="text">The text to display after all choices.</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetPostChoiceText(string text) {
			PostChoiceText = text;
			return this;
		}

		/// <summary>
		/// Sets YOffset and returns the menu.
		/// </summary>
		/// <param name="yOffset">The Y offset to draw the menu at.</param>
		/// <return>This menu.</return>
		public ConsoleSelectMenu SetYOffset(int yOffset) {
			YOffset = yOffset;
			return this;
		}
	}
}
