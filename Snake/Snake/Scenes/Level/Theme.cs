using Microsoft.Xna.Framework;

namespace Snake.Scenes.Level
{
    interface Theme
    {
        Color BackgroundColor { get; }
        Color ParticleColor(int index);
    }
}
