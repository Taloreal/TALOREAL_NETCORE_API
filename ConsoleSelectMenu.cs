using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API {

    public class ConsoleSelectMenu(bool loops, bool numbered, bool clearOnRefresh) {

        public const int PulseRate = 62; // milliseconds between key checks
        public const int MaxPulses = 15; // maximum number of heartbeats before refreshing the menu


        static readonly string[] QuitPhrases = [ "exit", "quit", "continue", "break", "back" ];

        
        public bool ClearConsole { get; private set; } = clearOnRefresh;
        public bool Loops { get; private set; } = loops;
        public bool Numbered { get; private set; } = numbered;

        private bool MenuOpen = false;

        private int _YOffset = 0;
        public int YOffset {
            get => _YOffset;
            private set => _YOffset = Math.Clamp(value, 0, Console.WindowHeight - 1);
        }


        public int Selected { get; private set; } = -1;


        private int _ExitChoice = -1;
        public int ExitChoice {
            get => _ExitChoice;
            private set { 
                _ExitChoice = Math.Clamp(value, -1, Choices.Count - 1);
            }
        }


        public string PreChoiceText { get; private set; } = "";
        public string PostChoiceText { get; private set; } = "";


        public event Action<ConsoleSelectMenu>? OnMenuOpen;
        public event Action<ConsoleSelectMenu>? OnDrawMenu;
        public event Action<ConsoleSelectMenu>? OnChoicesDisplayed;
        public event Action<ConsoleSelectMenu, int>? OnChoiceMade;
        public event Action<ConsoleSelectMenu>? OnMenuClosed;


        public int ChoiceCount => Choices.Count;
        private readonly List<ConsoleMenuItem> Choices = [];


        public ConsoleMenuItem this[int index] => Choices[index];

        public ConsoleSelectMenu AddChoice(ConsoleMenuItem item) {
            // Only change the menu if it's not open.
            if (MenuOpen == false) {
                Choices.Add(item);
                if (QuitPhrases.Contains(item.Text.ToLower())) {
                    ExitChoice = Choices.IndexOf(item);
                }
            }
            return this;
        }

        public void RemoveChoice(ConsoleMenuItem item) {
            // Only change the menu if it's not open.
            if (MenuOpen == false) {
                int ndx = Choices.IndexOf(item);
                if (ndx != -1) {
                    Choices.Remove(item);
                    if (ndx == ExitChoice) {
                        ExitChoice = -1; // reset exit choice if it was removed
                        // get the last choice that is a quit phrase
                        Choices.For((c, i) => {
                            if (QuitPhrases.Contains(c.Text.ToLower())) {
                                ExitChoice = i; // set the quit phrase as the new exit choice
                            }
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Displays the menu and allows the user to make a choice.
        /// </summary>
        /// <returns>The choice the user made or an error code: 
        /// -1 = menu error, 
        /// -2 = no choices, 
        /// -3 = Loops but has no exit option.</returns>
        public int GetChoice() {
            OnMenuOpen?.Invoke(this);
            MenuOpen = true;

            // If there are no choices, return -1
            if (Choices.Count < 1) { return -2; }
            // Validate the exit choice if loops are enabled
            if (Loops && (ExitChoice < 0 || ExitChoice >= Choices.Count)) { return -3; }

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
                    if (heartbeat >= MaxPulses) { continue; }

                    key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.UpArrow) { Selected = Math.Max(0, Selected - 1); }
                    if (key.Key == ConsoleKey.DownArrow) { Selected = Math.Min(Choices.Count - 1, Selected + 1); }
                    if (key.Key == ConsoleKey.Enter) {
                        chosen = true;
                        Choices[Selected].OnSelect();
                        OnChoiceMade?.Invoke(this, Selected);
                    }

                } while (chosen == false || (Loops && Selected != ExitChoice));
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

        private void DisplayMenu() {
            // clear the console if needed
            if (ClearConsole) { 
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

        private void DisplayMenuItem(ConsoleMenuItem choice, int ndx) {
            Exception? ex = null;
            // Save the original console colors
            (ConsoleColor ogText, ConsoleColor ogBack) = 
                (Console.ForegroundColor, Console.BackgroundColor);
            try {
                // Set the colors based on whether this choice is selected or not
                (Console.BackgroundColor, Console.ForegroundColor) = Selected != ndx ?
                    (choice.BackColor, choice.TextColor) :
                    (choice.TextColor, choice.BackColor);

                Console.WriteLine((Numbered ? (ndx + 1) + ": " : "") + choice.Text);
            }
            catch (Exception e) {
                Console.WriteLine("An error occurred while displaying the menu item.");
                ex = e;
            }
            // Reset the console colors to the original values
            (Console.ForegroundColor, Console.BackgroundColor) = (ogText, ogBack);
            // If an exception occurred, throw it and stop processing the menu
            if (ex != null) {
                throw ex;
            }
        }

        public ConsoleSelectMenu AddOnMenuOpenAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnMenuOpen += action;
            }
            return this;
        }

        public ConsoleSelectMenu RemoveOnMenuOpenAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnMenuOpen -= action;
            }
            return this;
        }

        public ConsoleSelectMenu AddOnDrawMenuAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnDrawMenu += action;
            }
            return this;
        }

        public ConsoleSelectMenu RemoveOnDrawMenuAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnDrawMenu -= action;
            }
            return this;
        }

        public ConsoleSelectMenu AddOnChoicesDisplayedAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnChoicesDisplayed += action;
            }
            return this;
        }

        public ConsoleSelectMenu RemoveOnChoicesDisplayedAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnChoicesDisplayed -= action;
            }
            return this;
        }

        public ConsoleSelectMenu AddOnChoiceMadeAction(Action<ConsoleSelectMenu, int>? action) {
            if (action != null) {
                OnChoiceMade += action;
            }
            return this;
        }

        public ConsoleSelectMenu RemoveOnChoiceMadeAction(Action<ConsoleSelectMenu, int>? action) {
            if (action != null) {
                OnChoiceMade -= action;
            }
            return this;
        }

        public ConsoleSelectMenu AddOnMenuClosedAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnMenuClosed += action;
            }
            return this;
        }

        public ConsoleSelectMenu RemoveOnMenuClosedAction(Action<ConsoleSelectMenu>? action) {
            if (action != null) {
                OnMenuClosed -= action;
            }
            return this;
        }

        public ConsoleSelectMenu SetClearConsole(bool clear) {
            ClearConsole = clear;
            return this;
        }

        public ConsoleSelectMenu SetLoops(bool loops) {
            Loops = loops;
            return this;
        }

        public ConsoleSelectMenu SetNumbered(bool numbered) {
            Numbered = numbered;
            return this;
        }

        public ConsoleSelectMenu SetExitChoice(int choice) {
            ExitChoice = choice;
            return this;
        }

        public ConsoleSelectMenu SetPreChoiceText(string text) {
            PreChoiceText = text;
            return this;
        }

        public ConsoleSelectMenu SetPostChoiceText(string text) {
            PostChoiceText = text;
            return this;
        }

        public ConsoleSelectMenu SetYOffset(int yOffset) {
            YOffset = yOffset;
            return this;
        }
    }
}
