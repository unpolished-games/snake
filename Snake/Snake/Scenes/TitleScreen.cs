using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Snake.Scenes
{
    internal class TitleScreen : Scene
    {

        public TitleScreen(IScenes scenes)
        {
            Texture2D titleScreen = null;
            SpriteBatch spriteBatch = null;

            LoadContent = content =>
            {
                titleScreen = content.Load<Texture2D>("title screen");
            };

            Update = (runtime, delta) =>
            {
                if (runtime > TimeSpan.FromSeconds(3) || Keyboard.GetState().GetPressedKeys().Length > 0)
                {
                    this.End();
                    scenes.Level.Begin();
                }
            };

            Draw = (graphics, basicEffect, gameTime) =>
            {
                spriteBatch = spriteBatch ?? new SpriteBatch(graphics.GraphicsDevice);

                var screen = new Vector2(graphics.GraphicsDevice.Viewport.Bounds.Width, graphics.GraphicsDevice.Viewport.Bounds.Height);

                var texture = new Vector2(titleScreen.Width, titleScreen.Height);

                spriteBatch.Begin();
                spriteBatch.Draw(titleScreen, (screen - texture) / 2, Color.White);
                spriteBatch.End();
            };
        }
    }
}
