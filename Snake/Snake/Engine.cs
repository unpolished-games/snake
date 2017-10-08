using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Snake
{
    interface IScenes
    {
        Scenes.Scene SplashScreen { get; }
        Scenes.Scene TitleScreen { get; }
        Scenes.Scene Level { get; }
    }

    internal class Engine: Microsoft.Xna.Framework.Game, IScenes
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;

        public Scenes.Scene SplashScreen { get; }
        public Scenes.Scene TitleScreen { get; }
        public Scenes.Scene Level { get; }

        public Engine()
        {
            SplashScreen = new Scenes.SplashScreen(this);
            TitleScreen = new Scenes.TitleScreen(this);
            Level = new Scenes.Level(this);

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Window.ClientBounds.Width,
                PreferredBackBufferHeight = Window.ClientBounds.Height,
                IsFullScreen = true
            };
            IsMouseVisible = false;
            Content.RootDirectory = "Content";
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        protected override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            SplashScreen.Begin();
        }

        private Scenes.Scene[] allScenes;
        private IEnumerable<Scenes.Scene> AllScenes
        {
            get
            {
                if (allScenes == null)
                {
                    var properties = typeof(IScenes).GetTypeInfo().DeclaredProperties;
                    var values = properties.Select(p => p.GetValue(this));
                    allScenes = values.Cast<Scenes.Scene>().ToArray();
                }
                return allScenes;
            }
        }
        private IEnumerable<Scenes.Scene> ActiveScenes => AllScenes.Where(s => s.Active);

        protected override void LoadContent()
        {
            base.LoadContent();

            foreach(var scene in AllScenes)
            {
                scene._LoadContent(Content);
            }
        }

        internal void SetNewHighscore(int highscore) => (Level as Scenes.Level).SetNewHighscore(highscore);

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
            if(ActiveScenes.Count() == 0)
            {
                Exit();
            }
            foreach(var scenes in ActiveScenes)
            {
                scenes._Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);
            foreach(var scene in ActiveScenes)
            {
                scene._Draw(graphics, basicEffect, gameTime);
            }
        }
    }
}