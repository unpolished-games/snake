using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

namespace Snake.Scenes
{
    internal class TitleScreen : Scene
    {
        Texture2D titleScreen;
        SpriteBatch spriteBatch;

        public Action<State> OnDraw { get; set; }

        public void Draw(GraphicsDeviceManager graphics, BasicEffect basicEffect, GameTime gameTime)
        {
            spriteBatch = spriteBatch ?? new SpriteBatch(graphics.GraphicsDevice);

            var screen = new Vector2(graphics.GraphicsDevice.Viewport.Bounds.Width, graphics.GraphicsDevice.Viewport.Bounds.Height);

            var texture = new Vector2(titleScreen.Width, titleScreen.Height);

            graphics.GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            spriteBatch.Draw(titleScreen, (screen - texture) / 2, Color.White);
            spriteBatch.End();
        }

        public void LoadContent(ContentManager content)
        {
            titleScreen = content.Load<Texture2D>("title screen");
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
