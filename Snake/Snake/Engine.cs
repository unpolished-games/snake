using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Snake
{
    internal class Engine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;

        Scenes.Scene current;
        Scenes.Scene[] scenes;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Window.ClientBounds.Width,
                PreferredBackBufferHeight = Window.ClientBounds.Height,
                IsFullScreen = true
            };
            IsMouseVisible = false;
            Content.RootDirectory = "Content";
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;

            scenes = new Scenes.Scene[]
            {
                new Scenes.SplashScreen(),
                new Scenes.TitleScreen(),
                new Scenes.Level()
            };

            async Task storyBook()
            {
                current = scenes[0];
                await Task.Delay(3775);
                current = scenes[1];
                await Task.Delay(3775);
                current = scenes[2];
            }
            var _ = storyBook();
        }

        protected override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            foreach(var scene in scenes)
            {
                scene.LoadContent(Content);

                scene.OnDraw = state => this.OnDraw?.Invoke(state);
            }
        }

        internal void SetNewHighscore(int highscore) => scenes.OfType<Scenes.Level>().First().SetNewHighscore(highscore);

        public Action<State> OnDraw { get; internal set; }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width || graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            current.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            current.Draw(graphics, basicEffect, gameTime);
        }
    }
}