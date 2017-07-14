using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Snake.Scenes
{
    internal class SplashScreen : Scene
    {
        Texture2D splashScreen;
        SpriteBatch spriteBatch;

        public Action<State> OnDraw { get; set; }

        public void Draw(GraphicsDeviceManager graphics, BasicEffect basicEffect, GameTime gameTime)
        {
            spriteBatch = spriteBatch ?? new SpriteBatch(graphics.GraphicsDevice);

            var screen = new Vector2(graphics.GraphicsDevice.Viewport.Bounds.Width, graphics.GraphicsDevice.Viewport.Bounds.Height);

            var texture = new Vector2(splashScreen.Width, splashScreen.Height);

            graphics.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(splashScreen, (screen - texture) / 2, Color.White);
            spriteBatch.End();
        }

        public void LoadContent(ContentManager content)
        {
            splashScreen = content.Load<Texture2D>("splash screen");
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
