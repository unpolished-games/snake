using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Snake.Scenes.Level
{
    class SimpleTheme : Theme
    {
        private Color backgroundColor;
        private List<Color[]> particleColors;
        Random r;

        public SimpleTheme(Color backgroundColor)
        {
            this.backgroundColor = backgroundColor;
            this.particleColors = new List<Color[]>();
            this.r = new Random();
        }
        public void Add(params Color[] fadedColors)
        {
            particleColors.Add(fadedColors);
        }

        public Color BackgroundColor => backgroundColor;

        public Color ParticleColor(int index)
        {
            var fadedIndex = index % particleColors.Count;
            var fadedColors = particleColors[fadedIndex];

            var sample0Index = (index / particleColors.Count) % fadedColors.Length;
            var sample1Index = ((index + 1) / particleColors.Count) % fadedColors.Length;
            var sample0 = fadedColors[sample0Index];
            var sample1 = fadedColors[sample1Index];

            var blend = ((index * 254522642) % 256) / 256f;

            return Color.Lerp(sample0, sample1, blend);
        }
    }
}
