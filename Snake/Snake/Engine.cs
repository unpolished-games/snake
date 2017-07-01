using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    internal class Engine : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        BasicEffect basicEffect;
        Texture2D glowingTile;

        SoundEffect eatingApple;
        SoundEffect dyingSnake;
        SoundEffect newHighscore;

        Random random;

        List<(Vector2 position, Vector2 direction, float age, Color color)> particles;

        Game game;
        State state;

        double timestamp;
        double milliseconds;
        double rate = 1000 / 15;

        Input bufferedInput = Input.None;

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

            particles = new List<(Vector2 position, Vector2 direction, float age, Color color)>();

            random = new Random();

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
            eatingApple = Content.Load<SoundEffect>("eating Apple");
            dyingSnake = Content.Load<SoundEffect>("dying Snake");
            newHighscore = Content.Load<SoundEffect>("new Highscore");

            game.OnMoment = moment =>
            {
                switch (moment)
                {
                    case Moment.EatingApple:
                        eatingApple.Play();
                        var a = new Vector2(state.apple.position.X, state.apple.position.Y);
                        for (var i = 0; i < 25; i++)
                        {
                            particles.Add((a + RandomDirection(0f, .9f), RandomDirection() * 0.8f, (float)random.NextDouble() * 0.35f + 0.45f, new Color(random.Next(180, 256), 0, 0)));
                            particles.Add((a + RandomDirection(0f, .7f), RandomDirection() * 0.6f, (float)random.NextDouble() * 0.45f + 0.45f, new Color(random.Next(140, 256), 0, 0)));
                            particles.Add((a + RandomDirection(0f, .5f), RandomDirection() * 0.1f, (float)random.NextDouble() * 0.65f + 0.45f, new Color(random.Next(170, 256), 150, 0)));
                            particles.Add((a + RandomDirection(0f, .3f), RandomDirection() * 0.2f, (float)random.NextDouble() * 0.75f + 0.45f, new Color(random.Next(150, 256), 0, 0)));
                        }
                        break;

                    case Moment.Dying:
                        dyingSnake.Play();
                        foreach(var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            for (var i = 0; i < 25; i++)
                            {
                                particles.Add((p + RandomDirection(.5f, .5f), RandomDirection(), (float)random.NextDouble() * 0.75f + 0.45f, new Color(0, random.Next(50, 100), 0)));
                                particles.Add((p + RandomDirection(.25f, .35f), RandomDirection() * 0.3f, (float)random.NextDouble() * 0.85f + 0.45f, new Color(0, random.Next(50, 100), 0)));
                                particles.Add((p + RandomDirection(0f, .45f), RandomDirection() * 0.1f, (float)random.NextDouble() * 0.95f + 0.35f, new Color(random.Next(100, 150), 120, 20)));
                            }
                            for (var i = 0; i < 2; i++)
                            {
                                if (random.Next(0, 10) > 6)
                                {
                                    particles.Add((p + RandomDirection(0f, .35f), RandomDirection() * 0f, (float)random.NextDouble() * 1.75f + 0.45f, new Color(255, 0, 0)));
                                }                            }
                        }
                        break;

                    case Moment.NewHighscore:
                        newHighscore.Play();
                        foreach (var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            for (var i = 0; i < 10; i++)
                            {
                                particles.Add((p + RandomDirection(.5f, .5f), RandomDirection(), (float)random.NextDouble() * 0.65f + 0.15f, new Color(random.Next(100, 250), random.Next(100, 250), random.Next(100, 250))));
                            }
                        }
                        break;

                    case Moment.Moving:
                        foreach (var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            if(random.Next(0, 10) > 8)
                            {
                                particles.Add((p + RandomDirection(.5f, .5f), RandomDirection() * .05f, (float)random.NextDouble() * .85f + 0.35f, new Color(random.Next(0, 100), random.Next(50, 250), random.Next(0, 25))));
                            }
                        }
                        break;

                }
            };
        }

        private Vector2 RandomDirection(float from = 1f, float to = 1f)
        {
            var next = random.NextDouble() * 2 * Math.PI;
            return new Vector2((float)Math.Sin(next), (float)Math.Cos(next)) * ((float)random.NextDouble() * (to - from) + from);
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

        internal void SetNewHighscore(int highscore)
        {
            state.highscore = Math.Max(state.highscore, highscore);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var keyboard = Keyboard.GetState();
            var input = bufferedInput;
            input = keyboard.IsKeyDown(Keys.Left) ? Input.Left : input;
            input = keyboard.IsKeyDown(Keys.Right) ? Input.Right : input;
            input = keyboard.IsKeyDown(Keys.Up) ? Input.Up : input;
            input = keyboard.IsKeyDown(Keys.Down) ? Input.Down : input;

            var gamepad = GamePad.GetState(PlayerIndex.One);
            input = gamepad.IsButtonDown(Buttons.DPadLeft) ? Input.Left : input;
            input = gamepad.IsButtonDown(Buttons.DPadRight) ? Input.Right : input;
            input = gamepad.IsButtonDown(Buttons.DPadUp) ? Input.Up : input;
            input = gamepad.IsButtonDown(Buttons.DPadDown) ? Input.Down : input;

            milliseconds += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (milliseconds > rate)
            {
                milliseconds -= rate;

                state = game.Update(state, input);

                if(keyboard.IsKeyDown(Keys.H))
                {
                    state.highscore = 0;
                }

                input = Input.None;
            }

            bufferedInput = input;

            particles = particles
            .Select(p => (
                position: p.position + p.direction * (float)gameTime.ElapsedGameTime.TotalSeconds,
                direction: p.direction + RandomDirection() * 0.1f,
                age: p.age - (float)gameTime.ElapsedGameTime.TotalSeconds,
                color: p.color
            ))
            .Where(p => p.age > 0)
            .ToList();

            timestamp = gameTime.TotalGameTime.TotalSeconds;

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
            DrawSquare(0, 0, 2, Color.Black, 0.001f);

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
                        DrawSquare(x, y, 15f / 16f, Color.Red, 0.15f);
                    }
                    else if (head)
                    {
                        DrawSquare(x, y, 15f / 16f, Color.LightGreen, 0.1f);
                    }
                    else if (tail)
                    {
                        foreach (var (link, index) in state.snake.links.Select((link, index) => (link: link, index: index)).Where(value => value.link == position))
                        {
                            var scale = 1f - 1f * index / state.snake.length;
                            DrawSquare(x, y, 1f - 1f / 4f * scale, Color.DarkGreen, 0.03f);
                        }
                    }
                }
            }
            foreach(var p in particles)
            {
                DrawSquare(p.position.X, p.position.Y, p.age * .1f, p.color, 0.2f);
            }
            base.Draw(gameTime);
            OnDraw(state);
        }

        private void DrawSquare(float x, float y, float scale, Color color, float factor)
        {
            scale *= 1 + factor * (float)Math.Sin(timestamp * 2.4f + x + y);
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