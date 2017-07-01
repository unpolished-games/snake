using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Snake
{
    public sealed partial class MainPage : Page
    {
        Game game;
        Queue<Key> keys;
        State state;
        Engine engine;

        public MainPage()
        {
            game = new Game(16);
            keys = new Queue<Key>();
            state = game.Init(0);
            this.InitializeComponent();
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(1000 / 60)
            };
            timer.Tick += OnTick;
            timer.Start();
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.ExtendViewIntoTitleBar = true;

            engine = MonoGame.Framework.XamlGame<Engine>.Create(string.Empty, Window.Current.CoreWindow, monogameRenderTarget);

        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.Left:
                    keys.Enqueue(Key.Left);
                    break;
                case VirtualKey.Up:
                    keys.Enqueue(Key.Up);
                    break;
                case VirtualKey.Right:
                    keys.Enqueue(Key.Right);
                    break;
                case VirtualKey.Down:
                    keys.Enqueue(Key.Down);
                    break;
            }
        }

        private void OnTick(object sender, object e)
        {
            var key = keys.Count > 0 ? keys.Peek() : Key.None;
            state = game.Update(state, key);
            keys.Clear();

            engine.SetState(state);

            Draw();
        }

        static SolidColorBrush DarkGreenBrush = new SolidColorBrush(Colors.DarkGreen);
        static SolidColorBrush GreenBrush = new SolidColorBrush(Colors.LightGreen);
        static SolidColorBrush RedBrush = new SolidColorBrush(Colors.Red);

        static Thickness DefaultThickness = new Thickness(1);

        private void Draw()
        {
            score.Text = $"{state.score}";
            highscore.Text = $"{state.highscore}";
        }

        private void Page_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().IsFullScreenMode == false)
            {
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
            else
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
        }
    }
}
