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
            Texture2D glowingTile = null;
            PixelFont pixelFont = null;
            int selectionIndex = 0;
            int selectionCount = 3;
            BackgroundParticles backgroundParticles = new BackgroundParticles();

            LoadContent = content =>
            {
                glowingTile = content.Load<Texture2D>("glowing Tile");
                pixelFont = new PixelFont();
            };

            Update = (input, delta) =>
            {
                backgroundParticles.Update(TimeSpan.FromTicks(delta.Ticks / 2));

                var keyboard = input.Keyboard;
                var touchPanel = input.TouchPanel;

                if (keyboard.WhenDown(Keys.Down))
                {
                    selectionIndex = Math.Min(selectionIndex + 1, selectionCount - 1);
                }
                else if (keyboard.WhenDown(Keys.Up))
                {
                    selectionIndex = Math.Max(selectionIndex - 1, 0);
                }
                else if (keyboard.WhenDown(Keys.Enter) || touchPanel.WhenTouching)
                {
                    switch(selectionIndex)
                    {
                        case 0:
                            scenes.Level.BeginScene(false);
                            this.PauseScene();
                            break;

                        case 1:
                            scenes.Level.BeginScene(true);
                            this.PauseScene();
                            break;

                        case 2:
                            this.EndScene();
                            break;
                    }
                }
            };

            Draw = engine =>
            {
                engine.ConfigureEffect(e =>
                {
                    e.Texture = glowingTile;
                });

                backgroundParticles.Each(p =>
                {
                    engine.DrawSquare(p.position.X, p.position.Y, p.age * .1f, p.color * .1f, 0.2f, Runtime);
                });

                var seconds = (float)Runtime.TotalSeconds;
                var heights = new float[]
                {
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * 1f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .8f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .6f),
                    MathHelper.SmoothStep(0, -.5f, (seconds - 2.5f) * .4f),
                };

                string getSelectionText(int index)
                {
                    var selections = new string[]
                    {
                        "1 PLAYER",
                        "2 PLAYERS",
                        "EXIT"
                    };
                    if (index == selectionIndex)
                    {
                        return $">>{selections[index]}<<";
                    }
                    else
                    {
                        return selections[index];
                    }
                }

                DrawAnimatedMessage(engine, pixelFont, "SNAKE", seconds, heights[3], 1f / 12f, Alignment.Centered);

                DrawAnimatedMessage(engine, pixelFont, getSelectionText(0), seconds - 1.5f, heights[2] + .6f, 1f / 48f, Alignment.Right);
                DrawAnimatedMessage(engine, pixelFont, getSelectionText(1), seconds - 1.7f, heights[1] + .9f, 1f / 48f, Alignment.Right);
                DrawAnimatedMessage(engine, pixelFont, getSelectionText(2), seconds - 1.9f, heights[0] + 1.2f, 1f / 48f, Alignment.Right);
            };
        }

        private void DrawAnimatedMessage(Engine engine, PixelFont pixelFont, string message, float seconds, float verticalShift, float scale, Alignment alignment)
        {
            var screenshift = ((float)engine.Window.ClientBounds.Width) / engine.Window.ClientBounds.Height;
            var textshift = -(float)((pixelFont.Width + 1) * message.Length - 1);
            if (alignment == Alignment.Left)
            {
                screenshift = -screenshift;
                textshift = 0;
            }
            if(alignment == Alignment.Right)
            {
                // no changes needed
            }
            if(alignment == Alignment.Centered)
            {
                screenshift = 0;
                textshift = textshift / 2;
            }
            engine.ConfigureEffect(e =>
            {
                e.World =
                    Matrix.CreateTranslation(textshift + .5f, -pixelFont.Height / 2f + .5f, 0)
                    * Matrix.CreateScale(scale)
                    * Matrix.CreateTranslation(screenshift, verticalShift, 0);
            });

            pixelFont.DrawString(message, (x, y) =>
            {
                var dx = x / (float)message.Length;
                var fade = Math.Max(0, Math.Min(1, (seconds - .5f) - dx / 3));
                var color = Grey((float)Math.Sqrt(fade));
                engine.DrawSquare(x, y, 1f, color, 0.1f + (float)Math.Pow(1.1f * Math.Abs(1 - fade), 20), Runtime);
            });
        }

        private void DrawAnimatedMessageCentered(Engine engine, PixelFont pixelFont, string message, float seconds, float verticalShift, float scale)
        {
            engine.ConfigureEffect(e =>
            {
                e.World =
                    Matrix.CreateTranslation(-((pixelFont.Width + 1) * message.Length - 1) / 2f + .5f, -pixelFont.Height / 2f + .5f, 0)
                    * Matrix.CreateScale(scale)
                    * Matrix.CreateTranslation(0, verticalShift, 0);
            });

            pixelFont.DrawString(message, (x, y) =>
            {
                var dx = x / (float)message.Length;
                var fade = Math.Max(0, Math.Min(1, (seconds - .5f) - dx / 3));
                var color = Grey((float)Math.Sqrt(fade));
                engine.DrawSquare(x, y, 1f, color, 0.1f + (float)Math.Pow(1.1f * Math.Abs(1 - fade), 20), Runtime);
            });
        }

        private Color Grey(float brightness) => new Color(brightness, brightness, brightness, brightness);
    }
}