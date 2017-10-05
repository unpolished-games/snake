using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Snake.Scenes
{
    internal class SplashScreen : Scene
    {
        public SplashScreen(IScenes scenes)
        {
            Texture2D splashScreen = null;
            SpriteBatch spriteBatch = null;

            LoadContent = content =>
            {
                splashScreen = content.Load<Texture2D>("splash screen");
            };

            Update = (runtime, delta) =>
            {
                if (runtime > TimeSpan.FromSeconds(3))
                {
                    this.End();
                    scenes.TitleScreen.Begin();
                }
            };

            Draw = (graphics, basicEffect, gameTime) =>
            {
                spriteBatch = spriteBatch ?? new SpriteBatch(graphics.GraphicsDevice);

                var screen = new Vector2(graphics.GraphicsDevice.Viewport.Bounds.Width, graphics.GraphicsDevice.Viewport.Bounds.Height);

                var texture = new Vector2(splashScreen.Width, splashScreen.Height);

                spriteBatch.Begin();
                spriteBatch.Draw(splashScreen, (screen - texture) / 2, Color.White);
                spriteBatch.End();
            };
        }
    }
}
