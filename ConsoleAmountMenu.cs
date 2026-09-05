using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TALOREAL_NETCORE_API {

    public class ConsoleAmountMenu {

        public int Selected { get; private set; } = 0;
        public int MaxCombinedValue { get; private set; }

        public string PreChoiceText = "";
        public string PostChoiceText = "";

        public event Action<ConsoleAmountMenu>?      OnDrawMenu;
        public event Action<ConsoleAmountMenu>?      OnChoicesDisplayed;
        public event Action<ConsoleAmountMenu, int>? OnChoiceMade;
        public event Action<ConsoleAmountMenu>?      OnMenuClosed;

        public int ItemCount => Items.Count;
        private readonly List<ConsoleAmountMenuItem> Items = [];

        public int TotalValue {
            get {
                int count = 0;
                Items.ForEach(i => count += i.Value);
                return count;
            }
        }

        public int MaxTextLength {
            get {
                int length = 0;
                Items.ForEach(i => length = Math.Max(length, i.Text.Length));
                return length;
            }
        }


        public ConsoleAmountMenu() {
            MaxCombinedValue = 0;
        }

        public ConsoleAmountMenu(int maxValue) {
            MaxCombinedValue = maxValue;
        }


        public ConsoleAmountMenuItem? this[int index] =>
            index < 0 || index >= Items.Count ? null : Items[index];
        

        public void AddItem(ConsoleAmountMenuItem item) { 
            Items.Add(item); 
        }

        public int[] GetValues() {
            if (Items.Count >= 1) {
                bool chosen = false, 
                    ogVisible = Console.CursorVisible;
                ConsoleKeyInfo key; 
                Console.CursorVisible = false;
                while ((Selected != Items.Count) || chosen == false) {
                    chosen = false;
                    DisplayMenu();
                    while (Console.KeyAvailable == false) {
                        Thread.Sleep(62);
                    }
                    key = Console.ReadKey(true);
                    int step = 1;
                    if ((key.Modifiers & ConsoleModifiers.Alt) == ConsoleModifiers.Alt) {
                        step = 1;
                    }
                    if ((key.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift) {
                        step = 10;
                    }
                    if ((key.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control) {
                        step = 100;
                    }
                    if (key.Key == ConsoleKey.UpArrow) {
                        Selected = Math.Max(0, Selected - 1);
                    }
                    if (key.Key == ConsoleKey.DownArrow) {
                        Selected = Math.Min(Items.Count, Selected + 1);
                    }
                    if (key.Key == ConsoleKey.Backspace || key.Key == ConsoleKey.LeftArrow) {
                        chosen = true;
                        if (Selected != Items.Count) {
                            Items[Selected].Value -= step;
                            Items[Selected].OnSelect();
                        }
                    }
                    if (key.Key == ConsoleKey.Enter || key.Key == ConsoleKey.RightArrow) {
                        chosen = true;
                        if (Selected != Items.Count && (MaxCombinedValue < 1 || TotalValue < MaxCombinedValue)) {
                            int increaseBy = step;
                            int roomLeft = MaxCombinedValue - TotalValue;
                            if (MaxCombinedValue >= 1 && roomLeft < step) {
                                increaseBy = roomLeft;
                            }
                            Items[Selected].Value += increaseBy;
                            Items[Selected].OnSelect();
                        }
                    }
                    if (chosen == true) { 
                        OnChoiceMade?.Invoke(this, Selected); 
                    }
                }
                Console.CursorVisible = ogVisible;
                OnMenuClosed?.Invoke(this);
                int[] values = new int[Items.Count];
                values.For(i => { values[i] = Items[i].Value; });
                return values;
            }
            return Array.Empty<int>();
        }

        private void DisplayMenu() {
            int ndx = 0;
            int padding = MaxTextLength + 1;
            ConsoleColor ogText = Console.ForegroundColor;
            ConsoleColor ogBack = Console.BackgroundColor;
            ConsoleExt.TryClear();
            OnDrawMenu?.Invoke(this);
            if (MaxCombinedValue > 0) {
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(TotalValue + " / " + MaxCombinedValue + " assigned");
            }
            if (PreChoiceText != "") { 
                Console.WriteLine(PreChoiceText); 
            }
            Items.ForEach(i => { 
                DisplayMenuItem(i, ndx == Selected, padding); ndx++; 
            });
            Console.BackgroundColor = Selected == Items.Count ? 
                ConsoleColor.White : ConsoleColor.Black;
            Console.ForegroundColor = Selected == Items.Count ? 
                ConsoleColor.Black : ConsoleColor.White;
            Console.WriteLine("Done");
            Console.BackgroundColor = ogBack;
            Console.ForegroundColor = ogText;
            if (PostChoiceText != "") { 
                Console.WriteLine(PostChoiceText); 
            }
            OnChoicesDisplayed?.Invoke(this);
        }

        private static void DisplayMenuItem(ConsoleAmountMenuItem item, bool selected, int padding) {
            Console.BackgroundColor = selected == false ? 
                item.BackColor : item.TextColor;
            Console.ForegroundColor = selected == false ? 
                item.TextColor : item.BackColor;
            Console.Write((item.Text).PadRight(padding));
            Console.BackgroundColor = selected == false ? 
                item.TextColor : item.BackColor;
            Console.ForegroundColor = selected == false ? 
                item.BackColor : item.TextColor;
            int tenths = item.GetPercentage() / 10;
            Console.Write("".PadRight(tenths));
            Console.BackgroundColor = selected == false ? 
                item.BackColor : item.TextColor;
            Console.ForegroundColor = selected == false ? 
                item.TextColor : item.BackColor;
            Console.WriteLine("".PadRight(11 - tenths) + item.Value + " / " + item.Maximum);
        }
    }
}
