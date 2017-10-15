using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Snake.Scenes
{
    internal class Level : Scene
    {
        double shake;
        private Random random = new Random();

        public Level(IScenes scenes)
        {
            ForegroundParticles foregroundParticles = default;
            BackgroundParticles backgroundParticles = default;
            Game game = default;
            State state = default;

            Texture2D glowingTile = default;
            PixelFont pixelFont = default;
            SoundEffect eatingApple = default;
            SoundEffect dyingSnake = default;
            SoundEffect newHighscore = default;

            TimeSpan bufferToNextTick = default;
            TimeSpan tickRate = default;

            Input bufferedInput = default;

            var field = new (Color color, float random)[16, 16];

            Begin = () =>
            {
                foregroundParticles = new ForegroundParticles();
                backgroundParticles = new BackgroundParticles();

                game = new Game(16);
                state = game.Init(0);

                for (var y = 0; y < 16; y++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        var color = Color.Black;
                        color.G = (byte)(255 * (float)random.NextDouble() * .03f);
                        color.R = (byte)(color.G * (float)random.NextDouble());
                        color.B = (byte)(color.R * (float)random.NextDouble());

                        field[x, y] = (color, (float)random.NextDouble());
                    }
                }

                bufferedInput = Input.None;
                bufferToNextTick = TimeSpan.Zero;
                tickRate = TimeSpan.FromMilliseconds(1000 / 15);

                game.OnMoment = moment =>
                {
                    if(moment == Moment.EatingApple)
                    {
                        eatingApple.Play();
                        foregroundParticles.AddEatenApple(new Vector2(state.apple.position.X, state.apple.position.Y));
                    }
                    if(moment == Moment.Dying)
                    {
                        dyingSnake.Play();
                        shake = 1f;
                        foregroundParticles.AddDyingSnake(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                    }
                    if(moment == Moment.NewHighscore)
                    {
                        newHighscore.Play();
                        foregroundParticles.AddSparklesToSnake(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                    }
                    if (moment == Moment.Moving)
                    {
                        foregroundParticles.AddSparklesToMovement(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                    }
                };
            };

            LoadContent = content =>
            {
                glowingTile = content.Load<Texture2D>("glowing Tile");
                eatingApple = content.Load<SoundEffect>("eating Apple");
                dyingSnake = content.Load<SoundEffect>("dying Snake");
                newHighscore = content.Load<SoundEffect>("new Highscore");

                pixelFont = new PixelFont();
            };

            Update = delta =>
            {
                var keyboard = Keyboard.GetState();
                if(keyboard.IsKeyDown(Keys.Escape))
                {
                    this.EndScene();
                }
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

                bufferToNextTick += delta;
                if (bufferToNextTick > tickRate)
                {
                    bufferToNextTick -= tickRate;
                    state = game.Update(state, input);
                    input = Input.None;
                }

                bufferedInput = input;

                foregroundParticles.Update(delta);
                backgroundParticles.Update(delta);

                shake /= 2;
                if (shake < 0.001f)
                {
                    shake = 0;
                }
            };

            Draw = engine =>
            {
                engine.ClearScreen(Color.DarkSlateBlue);

                var _shake = Math.Pow(shake, 0.125f);

                engine.ConfigureEffect(e =>
                {
                    e.View = Matrix.CreateTranslation(
                        0.02f * (float)(_shake * Math.Sin(Runtime.TotalSeconds * 74.1)),
                        0.02f * (float)(_shake * Math.Sin((Runtime.TotalSeconds + 23532) * 47.2)),
                        0
                    );
                });

                engine.ConfigureEffect(e =>
                {
                    e.Texture = glowingTile;
                    e.World = Matrix.Identity;
                });

                backgroundParticles.Each(p =>
                {
                    engine.DrawSquare(p.position.X, p.position.Y, p.age * .1f, p.color, 0.2f, Runtime);
                });
                //engine.DrawSquare(graphics.GraphicsDevice, 0, 0, 2, Color.Black, 0, Runtime);

                engine.ConfigureEffect(e =>
                {
                    e.World = Matrix.CreateTranslation(-7.5f, -7.5f, 0) * Matrix.CreateScale(1f / 8);
                });

                for (var y = 0; y < 16; y++)
                {
                    for (var x = 0; x < 16; x++)
                    {
                        var scale = (1 - 4 * Math.Max(0, (float)shake - .3f * field[x, y].random)) * Math.Min(1.01f, Math.Max(0f, (float)(Runtime.TotalSeconds - field[x, y].random)));
                        engine.DrawSquare(x, y, scale, field[x, y].color, 0.01f, Runtime);
                    }
                }

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
                            engine.DrawSquare(x, y, 15f / 16f, Color.Red, 0.15f, Runtime);
                        }
                        else if (head)
                        {
                            engine.DrawSquare(x, y, 15f / 16f, Color.LightGreen, 0.1f, Runtime);
                        }
                        else if (tail)
                        {
                            foreach (var (link, index) in state.snake.links.Select((link, index) => (link: link, index: index)).Where(value => value.link == position))
                            {
                                var scale = 1f - 1f * index / state.snake.length;
                                engine.DrawSquare(x, y, 1f - 1f / 4f * scale, Color.DarkGreen, 0.03f, Runtime);
                            }
                        }
                    }
                }

                foregroundParticles.Each(p =>
                {
                    engine.DrawSquare(p.position.X, p.position.Y, p.age * .3f, p.color, 0.1f, Runtime);
                });

                pixelFont.DrawString($"                    Highscore\n{state.score}\n{state.highscore}", (x, y) =>
                {
                    var scale = 1 / 8f;
                    engine.DrawSquare(x * scale, y * scale, scale, Color.White, 0.1f, Runtime);
                }, rightAlign: true);
            };
        }
    }
}