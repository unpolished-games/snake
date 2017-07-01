using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Snake
{
    internal class Engine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        Texture2D glowingTile;

        Game game;
        State state;

        public Action<State> OnDraw { get; internal set; }

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

            game = new Game(16);
            state = game.Init(0);
        }

        protected override void Initialize()
        {
            base.Initialize();
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            glowingTile = Content.Load<Texture2D>("glowing Tile");
        }

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
            var keyboard = Keyboard.GetState();
            var input = Input.None;
            input = keyboard.IsKeyDown(Keys.Left) ? Input.Left : input;
            input = keyboard.IsKeyDown(Keys.Right) ? Input.Right : input;
            input = keyboard.IsKeyDown(Keys.Up) ? Input.Up : input;
            input = keyboard.IsKeyDown(Keys.Down) ? Input.Down : input;

            var gamepad = GamePad.GetState(PlayerIndex.One);
            input = gamepad.IsButtonDown(Buttons.DPadLeft) ? Input.Left : input;
            input = gamepad.IsButtonDown(Buttons.DPadRight) ? Input.Right : input;
            input = gamepad.IsButtonDown(Buttons.DPadUp) ? Input.Up : input;
            input = gamepad.IsButtonDown(Buttons.DPadDown) ? Input.Down : input;
            state = game.Update(state, input);

        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.DarkViolet);

            var size = (width: graphics.PreferredBackBufferWidth, height: graphics.PreferredBackBufferHeight);

            if (size.height < size.width)
            {
                basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1f * size.height / size.width, 1, 1));
            }
            else
            {
                basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1, 1f * size.width / size.height, 1));
            }

            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = glowingTile;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            basicEffect.CurrentTechnique.Passes.First().Apply();

            basicEffect.World = Matrix.Identity;
            basicEffect.CurrentTechnique.Passes.First().Apply();
            DrawSquare(0, 0, 2, Color.Black);

            basicEffect.World = Matrix.CreateTranslation(-7.5f, -7.5f, 0) * Matrix.CreateScale(1f / 8);
            basicEffect.CurrentTechnique.Passes.First().Apply();

            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
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
            //OnDraw(state); // some threading issue i guess.. 
        }

        private void DrawSquare(int x, int y, float scale, Color color)
        {
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