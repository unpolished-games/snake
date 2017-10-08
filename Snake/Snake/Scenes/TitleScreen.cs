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
            Texture2D splashScreen = null;
            SpriteBatch spriteBatch = null;
            Texture2D glowingTile = null;
            PixelFont pixelFont = null;

            LoadContent = content =>
            {
                splashScreen = content.Load<Texture2D>("splash screen");
                glowingTile = content.Load<Texture2D>("glowing Tile");
                pixelFont = new PixelFont();
            };

            Update = (runtime, delta) =>
            {
                _runtime = runtime;
                if (/* runtime > TimeSpan.FromSeconds(3) || */ Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    this.End();
                    scenes.Level.Begin();
                }
            };

            Draw = (graphics, basicEffect, gameTime) =>
            {
                //spriteBatch = spriteBatch ?? new SpriteBatch(graphics.GraphicsDevice);

                //var screen = new Vector2(graphics.GraphicsDevice.Viewport.Bounds.Width, graphics.GraphicsDevice.Viewport.Bounds.Height);

                //var texture = new Vector2(splashScreen.Width, splashScreen.Height);

                //spriteBatch.Begin();
                //spriteBatch.Draw(splashScreen, (screen - texture) / 2, Color.White);
                //spriteBatch.End();

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

                var message = "SNAKE";

                basicEffect.World = 
                    Matrix.CreateTranslation(-pixelFont.Width * message.Length / 2f, -pixelFont.Height / 2f, 0)
                    * Matrix.CreateScale(1f / 12f);
                basicEffect.CurrentTechnique.Passes.First().Apply();

                var seconds = (float)_runtime.TotalSeconds;

                pixelFont.DrawString(message, (x, y) =>
                {
                    var dx = x / (float)message.Length;
                    var fade = Math.Max(0, Math.Min(1,  (seconds - .5f) - dx / 3));
                    var color = Grey((float)Math.Sqrt(fade));
                    DrawSquare(graphics.GraphicsDevice, x, y, 1f, color, (float)Math.Pow(1.1f * Math.Abs(1 - fade), 20));
                });
            };
        }

        private Color Grey(float brightness) => new Color(brightness, brightness, brightness);

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