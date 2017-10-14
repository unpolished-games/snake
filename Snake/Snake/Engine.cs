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

            SplashScreen._Begin();
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
            var size = (width: graphics.PreferredBackBufferWidth, height: graphics.PreferredBackBufferHeight);

            if (size.height < size.width)
            {
                basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1f * size.height / size.width, 1, 1));
            }
            else
            {
                basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1, 1f * size.width / size.height, 1));
            }

            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = true;
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            basicEffect.CurrentTechnique.Passes.First().Apply();

            basicEffect.World = Matrix.Identity;
            basicEffect.CurrentTechnique.Passes.First().Apply();

            foreach (var scene in ActiveScenes)
            {
                scene._Draw(this, graphics, basicEffect, gameTime);
            }
        }

        public void DrawSquare(float x, float y, float scale, Color color, float factor, TimeSpan runtime)
        {
            scale *= 1 + factor * (float)Math.Sin(runtime.TotalSeconds * 2.4f + x + y);
            var positions = new Vector3[]
            {
                new Vector3(x - scale, y - scale, 0),
                new Vector3(x - scale, y + scale, 0),
                new Vector3(x + scale, y - scale, 0),
                new Vector3(x + scale, y + scale, 0)
            }
            .ToArray();
            var vertices = new VertexPositionColorTexture[]
            {
                new VertexPositionColorTexture(positions[0], color, Vector2.Zero),
                new VertexPositionColorTexture(positions[1], color, Vector2.UnitX),
                new VertexPositionColorTexture(positions[2], color, Vector2.UnitY),
                new VertexPositionColorTexture(positions[3], color, Vector2.One)
            };
            graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
        }
    }
}