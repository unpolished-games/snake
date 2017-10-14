using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake.Scenes
{
    internal class Level : Scene
    {
        IScenes scenes;
        Texture2D glowingTile;

        PixelFont pixelFont;

        SoundEffect eatingApple;
        SoundEffect dyingSnake;
        SoundEffect newHighscore;

        Random random;

        ForegroundParticles foregroundParticles;
        BackgroundParticles backgroundParticles;

        Game game;
        State state;
        
        List<Vector2> touchPositions;

        double timestamp;
        double milliseconds;
        double rate = 1000 / 15;

        double shake;

        Input bufferedInput = Input.None;

        public Level(IScenes scenes)
        {
            this.scenes = scenes;
            
            touchPositions = new List<Vector2>();

            random = new Random();

            game = new Game(16);
            state = game.Init(0);

            LoadContent = content =>
            {
                glowingTile = content.Load<Texture2D>("glowing Tile");
                eatingApple = content.Load<SoundEffect>("eating Apple");
                dyingSnake = content.Load<SoundEffect>("dying Snake");
                newHighscore = content.Load<SoundEffect>("new Highscore");

                pixelFont = new PixelFont();
            };

            Update = (runtime, delta) =>
            {
                var keyboard = Keyboard.GetState();
                if(keyboard.IsKeyDown(Keys.Escape))
                {
                    this.End();
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
                
                milliseconds += delta.TotalMilliseconds;
                if (milliseconds > rate)
                {
                    milliseconds -= rate;

                    state = game.Update(state, input);

                    if (keyboard.IsKeyDown(Keys.H))
                    {
                        state.highscore = 0;
                    }

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

                timestamp = runtime.TotalSeconds;
            };

            Draw = (graphics, basicEffect, gameTime) =>
            {
                graphics.GraphicsDevice.Clear(Color.DarkSlateBlue);

                var size = (width: graphics.PreferredBackBufferWidth, height: graphics.PreferredBackBufferHeight);

                if (size.height < size.width)
                {
                    basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1f * size.height / size.width, 1, 1));
                }
                else
                {
                    basicEffect.Projection = Matrix.CreateScale(1, -1, 1) * Matrix.CreateScale(new Vector3(1, 1f * size.width / size.height, 1));
                }

                var _shake = Math.Pow(shake, 0.125f);
                basicEffect.View = Matrix.CreateTranslation(
                    0.02f * (float)(_shake * Math.Sin(timestamp * 74.1)),
                    0.02f * (float)(_shake * Math.Sin((timestamp + 23532) * 47.2)),
                    0
                );

                graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
                graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                basicEffect.VertexColorEnabled = true;
                basicEffect.TextureEnabled = true;
                basicEffect.Texture = glowingTile;
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                basicEffect.CurrentTechnique.Passes.First().Apply();

                basicEffect.World = Matrix.Identity;
                basicEffect.CurrentTechnique.Passes.First().Apply();

                backgroundParticles.Each(p =>
                {
                    DrawSquare(graphics.GraphicsDevice, p.position.X, p.position.Y, p.age * .1f, p.color, 0.2f);
                });
                DrawSquare(graphics.GraphicsDevice, 0, 0, 2, Color.Black, 0);

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
                            DrawSquare(graphics.GraphicsDevice, x, y, 15f / 16f, Color.Red, 0.15f);
                        }
                        else if (head)
                        {
                            DrawSquare(graphics.GraphicsDevice, x, y, 15f / 16f, Color.LightGreen, 0.1f);
                        }
                        else if (tail)
                        {
                            foreach (var (link, index) in state.snake.links.Select((link, index) => (link: link, index: index)).Where(value => value.link == position))
                            {
                                var scale = 1f - 1f * index / state.snake.length;
                                DrawSquare(graphics.GraphicsDevice, x, y, 1f - 1f / 4f * scale, Color.DarkGreen, 0.03f);
                            }
                        }
                    }
                }
                foregroundParticles.Each(p =>
                {
                    DrawSquare(graphics.GraphicsDevice, p.position.X, p.position.Y, p.age * .3f, p.color, 0.1f);
                });

                pixelFont.DrawString($"                    Highscore\n{state.score}\n{state.highscore}", (x, y) =>
                {
                    var scale = 1 / 8f;
                    DrawSquare(graphics.GraphicsDevice, x * scale, y * scale, scale, Color.White, 0.1f);
                }, rightAlign: true);
            };
        }

        public override void Begin()
        {
            base.Begin();

            foregroundParticles = new ForegroundParticles();
            backgroundParticles = new BackgroundParticles();
            
            touchPositions.Clear();

            game = new Game(16);
            state = game.Init(0);

            game.OnMoment = moment =>
            {
                switch (moment)
                {
                    case Moment.EatingApple:
                        eatingApple.Play();
                        foregroundParticles.AddEatenApple(new Vector2(state.apple.position.X, state.apple.position.Y));
                        break;

                    case Moment.Dying:
                        dyingSnake.Play();
                        shake = 1f;
                        foregroundParticles.AddDyingSnake(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                        break;

                    case Moment.NewHighscore:
                        newHighscore.Play();
                        foregroundParticles.AddSparklesToSnake(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                        break;

                    case Moment.Moving:
                        foregroundParticles.AddSparklesToMovement(state.snake.links.Select(l => new Vector2(l.X, l.Y)));
                        break;

                }
            };
        }

        internal void SetNewHighscore(int highscore)
        {
            state.highscore = Math.Max(state.highscore, highscore);
        }

        private void DrawSquare(GraphicsDevice graphicsDevice, float x, float y, float scale, Color color, float factor)
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
            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
        }
    }
}