using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;

namespace KbLayoutProtoWpf
{
    public partial class MainWindow : Window
    {
        private GlobalKeyboardHook hookWinDown = new GlobalKeyboardHook(Keys.LWin);
        private GlobalKeyboardHook hookCtrlUp = new GlobalKeyboardHook(Keys.LControlKey);
        private HotKey _showHotKey;
        private int currentLayer = 0;
        private KeyboardLayout keyboardLayout;

        public MainWindow()
        {
            InitializeComponent();

            HandleHotKeyStuff();
            this.Visibility = Visibility.Hidden;

            LoadInKeyboardLayout();
            SetPositionToCenterBottomish();           
        }

        private void SetPositionToCenterBottomish()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = Width;
            double windowHeight = Height;

            Left = (screenWidth - windowWidth) / 2;
            Top = screenHeight - windowHeight - (screenHeight * 0.05);
        }

        private void LoadInKeyboardLayout()
        {
            keyboardLayout = this.GetKeyboardLayouts("crkbd");

            foreach (var layer in keyboardLayout.Layers)
            {
                foreach (var key in layer.Value.Rows)
                {
                    this.AddSpaceToOneCharKeysAndReplaceNothingWithFanceyChar(key);
                }
            }
            //◌
            foreach (var row in keyboardLayout.Layers.First().Value.Rows)
            {
                contentControl.Inlines.AddRange(this.AddColorAndTab(row));
                contentControl.Inlines.Add(Environment.NewLine);
            }
        }

        private void HandleHotKeyStuff()
        {
            hookWinDown.KeyDown += HookWinDown_KeyDown;
            hookCtrlUp.KeyUp += HookCtrlUp_KeyUp;
            _showHotKey = new HotKey(Key.LWin, KeyModifier.Ctrl | KeyModifier.Win, OnHotKeyHandler);
            _showHotKey.Register();
            this.InputBindings.Add(new InputBinding(new RelayCommand(EnterVisible, () => true), new KeyGesture(Key.Escape)));

        }

        private void HookWinDown_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            this.MainWindow_OnPreviewMouseButtonDown(null, null);
            e.Handled = true;
        }
        private void HookCtrlUp_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            LeaveVisible();
        }
        private void OnHotKeyHandler(HotKey hotKey)
        {
            EnterVisible();
        }

        private void EnterVisible()
        {
            this.Visibility = Visibility.Visible;
            this.Topmost = true;
            this.Left = (SystemParameters.PrimaryScreenWidth / 2) - (this.Width / 2);
            this.Top = (SystemParameters.PrimaryScreenHeight) - (this.Height) - 100;
            _showHotKey.Unregister();
            hookWinDown.Hook();
            hookCtrlUp.Hook();
            ProcessFocus.FocusProcess();
        }
        private KeyboardLayout GetKeyboardLayouts(string name)
        {
            string jsonContent = File.ReadAllText(name + ".json");
            KeyboardLayout keyboardLayout = JsonSerializer.Deserialize<KeyboardLayout>(jsonContent);
            return keyboardLayout;
        }

        private void LeaveVisible()
        {
            this.Visibility = Visibility.Hidden;

            hookWinDown.Unhook();
            hookCtrlUp.Unhook();
            _showHotKey.Register();
        }
        private Brush[] foregrounds = { Brushes.Cyan, Brushes.Cyan, Brushes.Magenta, Brushes.GreenYellow, Brushes.Turquoise, Brushes.Turquoise, Brushes.Transparent, Brushes.Turquoise, Brushes.Turquoise, Brushes.GreenYellow, Brushes.Magenta, Brushes.Cyan, Brushes.Cyan, };
        private int[] shouldBeBold = new[] { 1, 2, 3, 4, 8, 9, 10, 11 };
        private Inline[] AddColorAndTab(string[] row)
        {
            var result = new Inline[row.Length];

            for (int keyIndex = 0; keyIndex < row.Length; keyIndex++)
            {
                var run = new Run(row[keyIndex] + "\t");
                run.Foreground = foregrounds[keyIndex];

                if (shouldBeBold.Contains(keyIndex))
                {
                    run.FontWeight = FontWeights.Bold;
                }

                result[keyIndex] = run;
            }

            return result;
        }

        private void AddSpaceToOneCharKeysAndReplaceNothingWithFanceyChar(string[] row)
        {
            for (var index = 0; index < row.Length; index++)
            {
                var key = row[index];
                if (key.Length == 1)
                {
                    row[index] = ' ' + key;
                }
                if (key.Length == 0)
                {
                    row[index] = " ◌";
                }

            }
        }
        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.AccentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        private void MainWindow_OnPreviewMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.currentLayer++;
            if (this.currentLayer > this.keyboardLayout.Layers.Count - 1)
                this.currentLayer = 0;

            contentControl.Inlines.Clear();
            foreach (var row in keyboardLayout.Layers.Take(this.currentLayer + 1).Last().Value.Rows)
            {
                contentControl.Inlines.AddRange(this.AddColorAndTab(row));
                contentControl.Inlines.Add(Environment.NewLine);
            }
            contentControl.UpdateLayout();
        }
    }
}
