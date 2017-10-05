using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Snake.Scenes
{
    internal abstract class Scene
    {
        public Action<State> OnDraw { get; set; }

        private bool active = false;
        public bool Active => active;

        private TimeSpan runtime;

        public virtual void Begin()
        {
            this.active = true;
            this.runtime = TimeSpan.Zero;
        }
        public virtual void End()
        {
            this.active = false;
        }

        protected Action<GraphicsDeviceManager, BasicEffect, GameTime> Draw { private get; set; }
        protected Action<ContentManager> LoadContent { private get; set; }
        protected Action<TimeSpan, TimeSpan> Update { private get; set; }

        public virtual void _Draw(GraphicsDeviceManager graphics, BasicEffect basicEffect, GameTime gameTime)
        {
            Draw(graphics, basicEffect, gameTime);
        }
        public virtual void _LoadContent(ContentManager content)
        {
            LoadContent(content);
        }
        public virtual void _Update(GameTime gameTime)
        {
            runtime += gameTime.ElapsedGameTime;
            Update(runtime, gameTime.ElapsedGameTime);
        }
    }
}