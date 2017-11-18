using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Scenes.Level
{
    interface Theme
    {
        Color BackgroundColor { get; }
        Color ParticleColor(int index);
    }

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

    class Themes
    {
        List<Theme> themes;

        public Themes(int levelsPerTheme)
        {
            this.LevelsPerTheme = levelsPerTheme;
            themes = new List<Theme>();

            var theme = default(SimpleTheme);

            theme = new SimpleTheme(Color.DarkOliveGreen);
            theme.Add(Color.Yellow, Color.Green, Color.Brown);

            themes.Add(theme);

            theme = new SimpleTheme(Color.DarkGray);
            theme.Add(Color.White, Color.LightGray);
            theme.Add(Color.Black, Color.DarkGray);
            theme.Add(Color.Gray);

            themes.Add(theme);
        }

        public int LevelsPerTheme { get; private set; }

        public Theme this[int level] => themes[Math.Min(themes.Count - 1, level / LevelsPerTheme)];
    }
}
