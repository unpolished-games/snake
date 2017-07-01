using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace Snake
{
    internal class Engine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        State state;

        public Engine()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = Window.ClientBounds.Width,
                PreferredBackBufferHeight = Window.ClientBounds.Height
            };
            IsMouseVisible = false;
            Content.RootDirectory = "Content";
            this.Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        protected override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
        }

        private void Window_ClientSizeChanged(object sender, System.EventArgs e)
        {
            if(graphics.PreferredBackBufferWidth != Window.ClientBounds.Width || graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
            {
                graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                graphics.ApplyChanges();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            var size = (width: graphics.PreferredBackBufferWidth, height: graphics.PreferredBackBufferHeight);

            if (size.height < size.width)
            {
                basicEffect.View = Matrix.CreateScale(new Vector3(1f * size.height / size.width, 1, 1));
            }
            else
            {
                basicEffect.View = Matrix.CreateScale(new Vector3(1, 1f * size.width / size.height, 1));
            }
            basicEffect.World = Matrix.CreateTranslation(-8, 8, 0) * Matrix.CreateScale(1f / 8);

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            basicEffect.VertexColorEnabled = true;
            basicEffect.CurrentTechnique.Passes.First().Apply();

            for(var y = 0; y < 16; y++)
            {
                for(var x = 0; x < 16; x++)
                {
                    var rect = (x0: x, y0: y, x1: x + 1, y1: y + 1);
                    var position = new System.Numerics.Vector2(x, y);
                    var tail = state.snake.links?.Any(link => link == position) ?? false;
                    var head = state.snake.position == position;
                    var apple = state.apple.position == position;
                    if (apple)
                    {
                        DrawSquare(x, y, 15f / 16f, Color.Red);
                    }
                    else if (head)
                    {
                        DrawSquare(x, y, 15f / 16f, Color.LightGreen);
                    }
                    else if (tail)
                    {
                        foreach (var (link, index) in state.snake.links.Select((link, index) => (link: link, index: index)).Where(value => value.link == position))
                        {
                            var scale = 1f - 1f * index / state.snake.length;
                            DrawSquare(x, y, 1f - 1f / 4f * scale, Color.DarkGreen);
                        }
                    }
                }
            }
            base.Draw(gameTime);
        }

        private void DrawSquare(int x, int y, float scale, Color color)
        {
            var vertices = new Vector3[]
            {
                Vector3.Zero,
                new Vector3(0, 1, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0)
            }
            .Select(p => (p - new Vector3(0.5f)) * scale + new Vector3(0.5f))
            .Select(p => (p + new Vector3(x, y, 0)) * (Vector3.Down + Vector3.Right))
            .Select(p => new VertexPositionColor(p, color))
            .ToArray();
            graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
        }

        internal void SetState(State state)
        {
            this.state = state;
        }
    }
}
