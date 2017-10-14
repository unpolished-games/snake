﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace Snake.Scenes
{
    internal abstract class Scene
    {
        private bool active = false;
        public bool Active => active;

        private TimeSpan runtime;

        public virtual void _Begin()
        {
            this.active = true;
            this.runtime = TimeSpan.Zero;
            Begin?.Invoke();
        }
        public virtual void _End()
        {
            this.active = false;
        }

        protected Action Begin { private get; set; }
        protected Action<Engine> Draw { private get; set; }
        protected Action<ContentManager> LoadContent { private get; set; }
        protected Action<TimeSpan, TimeSpan> Update { private get; set; }

        public void _Draw(Engine engine)
        {
            Draw(engine);
        }
        public void _LoadContent(ContentManager content)
        {
            LoadContent?.Invoke(content);
        }
        public void _Update(GameTime gameTime)
        {
            runtime += gameTime.ElapsedGameTime;
            Update(runtime, gameTime.ElapsedGameTime);
        }
    }
}