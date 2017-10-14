using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace Snake.Scenes
{
    internal class TitleScreen : Scene
    {
        TimeSpan _runtime;

        public TitleScreen(IScenes scenes)
        {
            Texture2D glowingTile = null;
            PixelFont pixelFont = null;
            int selectionIndex = 0;
            int selectionCount = 3;
            KeyboardState lastKeyboard = default(KeyboardState);
            BackgroundParticles backgroundParticles = new BackgroundParticles();

            LoadContent = content =>
            {
                glowingTile = content.Load<Texture2D>("glowing Tile");
                pixelFont = new PixelFont();
            };

            Update = (runtime, delta) =>
            {
                _runtime = runtime;
                backgroundParticles.Update(delta / 2);
                var keyboard = Keyboard.GetState();
                if (keyboard.IsKeyDown(Keys.Down) && lastKeyboard.IsKeyUp(Keys.Down))
                {
                    selectionIndex = Math.Min(selectionIndex + 1, selectionCount);
                }
                else if (keyboard.IsKeyDown(Keys.Up) && lastKeyboard.IsKeyUp(Keys.Up))
                {
                    selectionIndex = Math.Max(selectionIndex - 1, 0);
                }
                else if (keyboard.IsKeyDown(Keys.Enter) && lastKeyboard.IsKeyDown(Keys.Enter))
                {
                    switch(selectionIndex)
                    {
                        case 0:
                            scenes.Level.Begin();
                            break;

                        case 1:
                            break;

                        case 2:
                            this.End();
                            break;
                    }
                }
                lastKeyboard = keyboard;
            };

            Draw = (graphics, basicEffect, gameTime) =>
            {
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
                basicEffect.Texture = glowingTile;
                graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                basicEffect.CurrentTechnique.Passes.First().Apply();

                basicEffect.World = Matrix.Identity;
                basicEffect.CurrentTechnique.Passes.First().Apply();

                backgroundParticles.Each(p =>
                {
                    DrawSquare(graphics.GraphicsDevice, p.position.X, p.position.Y, p.age * .1f, p.color * .2f, 0.2f);
                });

                var seconds = (float)_runtime.TotalSeconds;
                var heights = new float[]
                {
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * 1f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .8f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .6f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .4f),
                };

                string getSelectionText(int index)
                {
                    var selections = new string[]
                    {
                        "START GAME",
                        "OPTIONS",
                        "EXIT"
                    };
                    if (index == selectionIndex)
                    {
                        return $">>{selections[index]}<<";
                    }
                    else
                    {
                        return selections[index];
                    }
                }

                DrawAnimatedMessage(graphics, basicEffect, pixelFont, "SNAKE", seconds, heights[3], 1f / 12f);

                DrawAnimatedMessage(graphics, basicEffect, pixelFont, getSelectionText(0), seconds - 1.5f, heights[2] + .6f, 1f / 48f);
                DrawAnimatedMessage(graphics, basicEffect, pixelFont, getSelectionText(1), seconds - 1.7f, heights[1] + .9f, 1f / 48f);
                DrawAnimatedMessage(graphics, basicEffect, pixelFont, getSelectionText(2), seconds - 1.9f, heights[0] + 1.2f, 1f / 48f);
            };
        }

        private void DrawAnimatedMessage(GraphicsDeviceManager graphics, BasicEffect basicEffect, PixelFont pixelFont, string message, float seconds, float verticalShift, float scale)
        {
            basicEffect.World =
                Matrix.CreateTranslation(-((pixelFont.Width + 1) * message.Length - 1) / 2f + .5f, -pixelFont.Height / 2f + .5f, 0)
                * Matrix.CreateScale(scale)
                * Matrix.CreateTranslation(0, verticalShift, 0);
            basicEffect.CurrentTechnique.Passes.First().Apply();

            pixelFont.DrawString(message, (x, y) =>
            {
                var dx = x / (float)message.Length;
                var fade = Math.Max(0, Math.Min(1, (seconds - .5f) - dx / 3));
                var color = Grey((float)Math.Sqrt(fade));
                DrawSquare(graphics.GraphicsDevice, x, y, 1f, color, 0.1f + (float)Math.Pow(1.1f * Math.Abs(1 - fade), 20));
            });
        }

        private Color Grey(float brightness) => new Color(brightness, brightness, brightness, brightness);

        private void DrawSquare(GraphicsDevice graphicsDevice, float x, float y, float scale, Color color, float factor)
        {
            scale *= 1 + factor * (float)Math.Sin(_runtime.TotalSeconds * 2.4f + x + y);
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