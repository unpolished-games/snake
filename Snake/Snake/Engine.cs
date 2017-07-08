using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake
{
    internal class Engine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;

        Scenes.Level level;

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

            level = new Scenes.Level();
            level.OnDraw = state => this.OnDraw?.Invoke(state);
        }

        protected override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            level.LoadContent(Content);
        }

        internal void SetNewHighscore(int highscore) => level.SetNewHighscore(highscore);

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
            level.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            level.Draw(graphics, basicEffect, gameTime);
        }
    }
}