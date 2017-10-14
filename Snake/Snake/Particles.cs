using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    class Particles
    {
        protected Random random;
        List<(Vector2 position, Vector2 direction, float age, Color color)> particles;

        public Particles()
        {
            particles = new List<(Vector2 position, Vector2 direction, float age, Color color)>();
            random = new Random();
        }

        public virtual void Update(TimeSpan delta)
        {
            particles = particles
            .Select(p => (
                position: p.position + p.direction * (float)delta.TotalSeconds,
                direction: p.direction + RandomDirection() * 0.1f,
                age: p.age - (float)delta.TotalSeconds,
                color: p.color
            ))
            .Where(p => p.age > 0)
            .ToList();
        }

        public void Each(Action<(Vector2 position, Vector2 direction, float age, Color color)> action)
        {
            foreach(var particle in particles)
            {
                action(particle);
            }
        }

        public void Add(params (Vector2 position, Vector2 direction, float age, Color color)[] particles)
        {
            this.particles.AddRange(particles);
        }

        public int Count => particles.Count;

        protected Vector2 RandomDirection(float from = 1f, float to = 1f)
        {
            var next = random.NextDouble() * 2 * Math.PI;
            return new Vector2((float)Math.Sin(next), (float)Math.Cos(next)) * ((float)random.NextDouble() * (to - from) + from);
        }
    }
}
