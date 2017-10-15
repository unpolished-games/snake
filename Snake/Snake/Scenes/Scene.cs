using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

namespace Snake.Scenes
{
    internal abstract class Scene
    {
        private bool active = false;
        public bool Active => active;

        private TimeSpan runtime;
        protected TimeSpan Runtime => runtime;

        public virtual void BeginScene()
        {
            this.active = true;
            this.runtime = TimeSpan.Zero;
            Begin?.Invoke();
        }
        public virtual void EndScene()
        {
            this.active = false;
        }

        protected Action Begin { private get; set; }
        protected Action<Engine> Draw { private get; set; }
        protected Action<ContentManager> LoadContent { private get; set; }
        protected Action<TimeSpan> Update { private get; set; }

        public void DrawScene(Engine engine)
        {
            Draw(engine);
        }
        public void LoadContentForScene(ContentManager content)
        {
            LoadContent?.Invoke(content);
        }
        public void UpdateScene(GameTime gameTime)
        {
            runtime += gameTime.ElapsedGameTime;
            Update(gameTime.ElapsedGameTime);
        }
    }
}