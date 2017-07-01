using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Snake
{
    public sealed partial class MainPage : Page
    {
        Queue<Input> keys;
        Engine engine;
        State state;

        public MainPage()
        {
            keys = new Queue<Input>();
            this.InitializeComponent();
            var titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.ExtendViewIntoTitleBar = true;

            engine = MonoGame.Framework.XamlGame<Engine>.Create(string.Empty, Window.Current.CoreWindow, monogameRenderTarget);
            engine.OnDraw = state =>
            {
                if(state.score != this.state.score || state.highscore != this.state.highscore)
                {
                    var _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
                    {
                        score.Text = $"{state.score}";
                        highscore.Text = $"{state.highscore}";
                    }));
                    this.state = state;
                }
            };
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
