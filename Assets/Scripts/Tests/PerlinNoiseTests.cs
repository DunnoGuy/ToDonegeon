using System;
using NUnit.Framework;
using ToDonegeon.Generation;

namespace ToDonegeon.Tests
{
    public class PerlinNoiseTests
    {
        private const float Step = 0.13f;
        private const int Samples = 48;

        [Test]
        public void Same_Seed_Produces_Identical_Field()
        {
            var a = new PerlinNoise(2024);
            var b = new PerlinNoise(2024);
            for (int i = 0; i < Samples; i++)
            {
                float x = i * Step;
                float y = (Samples - i) * Step;
                Assert.That(a.Noise(x, y), Is.EqualTo(b.Noise(x, y)));
            }
        }

        [Test]
        public void Different_Seeds_Produce_Different_Fields()
        {
            var a = new PerlinNoise(1);
            var b = new PerlinNoise(2);
            bool anyDifference = false;
            for (int i = 0; i < Samples && !anyDifference; i++)
                if (Math.Abs(a.Noise(i * Step, i * Step) - b.Noise(i * Step, i * Step)) > 1e-6f)
                    anyDifference = true;

            Assert.That(anyDifference, Is.True);
        }

        [Test]
        public void Noise_Stays_In_The_Unit_Range()
        {
            var noise = new PerlinNoise(7);
            for (int gy = 0; gy < Samples; gy++)
            {
                for (int gx = 0; gx < Samples; gx++)
                {
                    float value = noise.Noise(gx * Step, gy * Step);
                    Assert.That(value, Is.InRange(0f, 1f));

                    float fractal = noise.Fractal(gx * Step, gy * Step, 4, 0.5f, 2f);
                    Assert.That(fractal, Is.InRange(0f, 1f));
                }
            }
        }

        [Test]
        public void Neighbouring_Samples_Change_Gently()
        {
            var noise = new PerlinNoise(11);
            const float near = 0.02f;
            for (int gy = 0; gy < Samples; gy++)
            {
                for (int gx = 0; gx < Samples; gx++)
                {
                    float here = noise.Noise(gx * Step, gy * Step);
                    float right = noise.Noise(gx * Step + near, gy * Step);
                    float up = noise.Noise(gx * Step, gy * Step + near);
                    Assert.That(Math.Abs(right - here), Is.LessThan(0.1f));
                    Assert.That(Math.Abs(up - here), Is.LessThan(0.1f));
                }
            }
        }

        [Test]
        public void Field_Is_Not_Constant()
        {
            var noise = new PerlinNoise(3);
            float min = 1f, max = 0f;
            for (int gy = 0; gy < Samples; gy++)
                for (int gx = 0; gx < Samples; gx++)
                {
                    float value = noise.Noise(gx * Step, gy * Step);
                    if (value < min) min = value;
                    if (value > max) max = value;
                }

            Assert.That(max - min, Is.GreaterThan(0.3f));
        }

        [Test]
        public void Fractal_Rejects_Nonpositive_Octaves()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PerlinNoise(0).Fractal(1f, 1f, 0, 0.5f, 2f));
        }
    }
}
