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

        SoundEffect eatingApple;
        SoundEffect dyingSnake;
        SoundEffect newHighscore;

        SoundEffect album01;

        Random random;

        List<(Vector2 position, Vector2 direction, float age, Color color)> foregroundParticles;
        List<(Vector2 position, Vector2 direction, float age, Color color)> backgroundParticles;

        Game game;
        State state;

        List<Vector2> mousePositions;
        List<Vector2> touchPositions;

        double timestamp;
        double milliseconds;
        double rate = 1000 / 15;

        double shake;

        Input bufferedInput = Input.None;

        public new Action<State> OnDraw { get; set; }

        public Level(IScenes scenes)
        {
            this.scenes = scenes;

            foregroundParticles = new List<(Vector2 position, Vector2 direction, float age, Color color)>();
            backgroundParticles = new List<(Vector2 position, Vector2 direction, float age, Color color)>();

            mousePositions = new List<Vector2>();
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

                //album01 = content.Load<SoundEffect>("Album/01");

                //var player = album01.CreateInstance();
                //player.IsLooped = true;
                //player.Volume = .25f;
                //player.Play();
            };

            Update = (runtime, delta) =>
            {
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

                var mouse = Mouse.GetState();
                if (mouse.LeftButton == ButtonState.Released)
                {
                    mousePositions.Clear();
                }
                if (mouse.LeftButton == ButtonState.Pressed)
                {
                    var position = new Vector2(mouse.X, mouse.Y);
                    mousePositions.Add(position);
                    mousePositions.Reverse();
                    mousePositions = mousePositions.Take(10).ToList();
                    mousePositions.Reverse();

                    if (mousePositions.Count == 10)
                    {
                        var direction = position - mousePositions.First();
                        var normalizedDirection = Vector2.Normalize(direction);
                        var leftRight = Vector2.Dot(normalizedDirection, -Vector2.UnitX);
                        var upDown = Vector2.Dot(normalizedDirection, -Vector2.UnitY);
                        input = Math.Abs(leftRight) >= Math.Abs(upDown) && leftRight > 0 ? Input.Left : input;
                        input = Math.Abs(leftRight) >= Math.Abs(upDown) && leftRight < 0 ? Input.Right : input;
                        input = Math.Abs(leftRight) < Math.Abs(upDown) && upDown > 0 ? Input.Up : input;
                        input = Math.Abs(leftRight) < Math.Abs(upDown) && upDown < 0 ? Input.Down : input;
                    }
                }

                var touchpanel = TouchPanel.GetState();
                if (touchpanel.Any())
                {
                    var touch = touchpanel.First();

                    var position = touch.Position;
                    touchPositions.Add(position);
                    touchPositions.Reverse();
                    touchPositions = touchPositions.Take(20).ToList();
                    touchPositions.Reverse();

                    if (touchPositions.Count == 20)
                    {
                        var direction = position - touchPositions.First();
                        var normalizedDirection = Vector2.Normalize(direction);
                        var leftRight = Vector2.Dot(normalizedDirection, -Vector2.UnitX);
                        var upDown = Vector2.Dot(normalizedDirection, -Vector2.UnitY);
                        var cosineOf22dot5Degrees = 0.92387953251128675612818318939679f;
                        var touchInput = Input.None;
                        touchInput = leftRight > cosineOf22dot5Degrees ? Input.Left : touchInput;
                        touchInput = leftRight < -cosineOf22dot5Degrees ? Input.Right : touchInput;
                        touchInput = upDown > cosineOf22dot5Degrees ? Input.Up : touchInput;
                        touchInput = upDown < -cosineOf22dot5Degrees ? Input.Down : touchInput;
                        if (touchInput != Input.None)
                        {
                            input = touchInput;
                            touchPositions.Clear();
                        }
                    }
                }
                else
                {
                    touchPositions.Clear();
                }

                milliseconds += delta.TotalMilliseconds;
                if (milliseconds > rate)
                {
                    milliseconds -= rate;

                    state = game.Update(state, input);

                    //AddGlitter(random.Next(2, 5), chance: .02f, position: RandomDirection() * 5, explode: true);

                    if (keyboard.IsKeyDown(Keys.H))
                    {
                        state.highscore = 0;
                    }

                    input = Input.None;
                }

                bufferedInput = input;

                foregroundParticles = foregroundParticles
                .Select(p => (
                    position: p.position + p.direction * (float)delta.TotalSeconds,
                    direction: p.direction + RandomDirection() * 0.1f,
                    age: p.age - (float)delta.TotalSeconds,
                    color: p.color
                ))
                .Where(p => p.age > 0)
                .ToList();

                backgroundParticles = backgroundParticles
                .Select(p => (
                    position: p.position + p.direction * (float)delta.TotalSeconds,
                    direction: p.direction + RandomDirection() * p.direction.Length() * 0.1f,
                    age: p.age - (float)delta.TotalSeconds,
                    color: p.color
                ))
                .Where(p => p.age > 0)
                .ToList();

                AddBackgroundParticlesAsNeeded();

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

                foreach (var p in backgroundParticles)
                {
                    DrawSquare(graphics.GraphicsDevice, p.position.X, p.position.Y, p.age * .1f, p.color, 0.2f);
                }
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
                foreach (var p in foregroundParticles)
                {
                    DrawSquare(graphics.GraphicsDevice, p.position.X, p.position.Y, p.age * .3f, p.color, 0.1f);
                }
                OnDraw?.Invoke(state);
            };
        }

        public override void Begin()
        {
            base.Begin();

            foregroundParticles.Clear();
            backgroundParticles.Clear();

            mousePositions.Clear();
            touchPositions.Clear();

            game = new Game(16);
            state = game.Init(0);

            AddGlitter(150);

            AddBackgroundParticlesAsNeeded();

            game.OnMoment = moment =>
            {
                switch (moment)
                {
                    case Moment.EatingApple:
                        eatingApple.Play();
                        var a = new Vector2(state.apple.position.X, state.apple.position.Y);
                        for (var i = 0; i < 25; i++)
                        {
                            foregroundParticles.Add((a + RandomDirection(0f, .9f), RandomDirection() * 0.8f, (float)random.NextDouble() * 0.35f + 0.45f, new Color(random.Next(180, 256), 0, 0)));
                            foregroundParticles.Add((a + RandomDirection(0f, .7f), RandomDirection() * 0.6f, (float)random.NextDouble() * 0.45f + 0.45f, new Color(random.Next(140, 256), 0, 0)));
                            foregroundParticles.Add((a + RandomDirection(0f, .5f), RandomDirection() * 0.1f, (float)random.NextDouble() * 0.65f + 0.45f, new Color(random.Next(170, 256), 150, 0)));
                            foregroundParticles.Add((a + RandomDirection(0f, .3f), RandomDirection() * 0.2f, (float)random.NextDouble() * 0.75f + 0.45f, new Color(random.Next(150, 256), 0, 0)));
                        }
                        break;

                    case Moment.Dying:
                        dyingSnake.Play();
                        shake = 1f;
                        foreach (var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            for (var i = 0; i < 25; i++)
                            {
                                foregroundParticles.Add((p + RandomDirection(.5f, .5f), RandomDirection(), (float)random.NextDouble() * 0.75f + 0.45f, new Color(0, random.Next(50, 100), 0)));
                                foregroundParticles.Add((p + RandomDirection(.25f, .35f), RandomDirection() * 0.3f, (float)random.NextDouble() * 0.85f + 0.45f, new Color(0, random.Next(50, 100), 0)));
                                foregroundParticles.Add((p + RandomDirection(0f, .45f), RandomDirection() * 0.1f, (float)random.NextDouble() * 0.95f + 0.35f, new Color(random.Next(100, 150), 120, 20)));
                            }
                            for (var i = 0; i < 2; i++)
                            {
                                if (random.Next(0, 10) > 6)
                                {
                                    foregroundParticles.Add((p + RandomDirection(0f, .35f), RandomDirection() * 0f, (float)random.NextDouble() * 1.75f + 0.45f, new Color(255, 0, 0)));
                                }
                            }
                        }
                        break;

                    case Moment.NewHighscore:
                        newHighscore.Play();
                        foreach (var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            for (var i = 0; i < 10; i++)
                            {
                                foregroundParticles.Add((p + RandomDirection(.5f, .5f), RandomDirection(), (float)random.NextDouble() * 0.65f + 0.15f, new Color(random.Next(100, 250), random.Next(100, 250), random.Next(100, 250))));
                            }
                        }
                        break;

                    case Moment.Moving:
                        foreach (var p in state.snake.links.Select(l => new Vector2(l.X, l.Y)))
                        {
                            if (random.Next(0, 10) > 8)
                            {
                                foregroundParticles.Add((p + RandomDirection(.5f, .5f), RandomDirection() * .05f, (float)random.NextDouble() * .85f + 0.35f, new Color(random.Next(0, 100), random.Next(50, 250), random.Next(0, 25))));
                            }
                        }
                        break;

                }
            };
        }

        private void AddGlitter(int times, float chance = 1f, Vector2? position = null, bool explode = false)
        {
            if (random.NextDouble() < chance)
            {
                var p = position ?? Vector2.Zero;
                p += new Vector2(8, 8);
                for (var i = 0; i < times; i++)
                {
                    var f = (p: explode ? 1f : 10f, s: explode ? 10f : 1f);
                    foregroundParticles.Add((p + f.p * RandomDirection(0f, 1.3f), f.s * RandomDirection() * 0.8f, (float)random.NextDouble() * 0.35f + 1.85f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    foregroundParticles.Add((p + f.p * RandomDirection(0f, 1.1f), f.s * RandomDirection() * 0.6f, (float)random.NextDouble() * 0.45f + 1.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    foregroundParticles.Add((p + f.p * RandomDirection(0f, .9f), f.s * RandomDirection() * 0.1f, (float)random.NextDouble() * 0.65f + 0.85f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    foregroundParticles.Add((p + f.p * RandomDirection(0f, .7f), f.s * RandomDirection() * 0.2f, (float)random.NextDouble() * 0.75f + 0.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                }
            }
        }

        private void AddBackgroundParticlesAsNeeded()
        {
            while(backgroundParticles.Count < 140)
            {
                for (var i = 0; i < 5; i++)
                {
                    var p = RandomDirection(0f, 1f);
                    backgroundParticles.Add((p + RandomDirection(0f, 1.3f), 0.1f * (p + RandomDirection(0f, 0.2f)), (float)random.NextDouble() * 0.35f + 3.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    backgroundParticles.Add((p + RandomDirection(0f, 1.1f), 0.4f * (p + RandomDirection(0f, 0.2f)), (float)random.NextDouble() * 0.45f + 2.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    backgroundParticles.Add((p + RandomDirection(0f, .9f), 0.7f * (p + RandomDirection(0f, 0.2f)), (float)random.NextDouble() * 0.65f + 1.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                    backgroundParticles.Add((p + RandomDirection(0f, .7f), 1f * (p + RandomDirection(0f, 0.2f)), (float)random.NextDouble() * 0.75f + 0.45f, new Color(random.Next(140, 256), random.Next(140, 256), random.Next(140, 256))));
                }
            }
        }

        private Vector2 RandomDirection(float from = 1f, float to = 1f)
        {
            var next = random.NextDouble() * 2 * Math.PI;
            return new Vector2((float)Math.Sin(next), (float)Math.Cos(next)) * ((float)random.NextDouble() * (to - from) + from);
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