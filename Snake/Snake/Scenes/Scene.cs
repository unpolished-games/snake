using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Snake.Scenes
{
    internal interface Scene
    {
        Action<State> OnDraw { get; }

        void Draw(GraphicsDeviceManager graphics, BasicEffect basicEffect, GameTime gameTime);
        void LoadContent(ContentManager content);
        void Update(GameTime gameTime);
    }
}