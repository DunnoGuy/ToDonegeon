using System;

namespace ToDonegeon.Generation
{
    // 2D gradient (Perlin) noise from a seeded permutation. Nearby points give nearby values.
    public sealed class PerlinNoise
    {
        private const int TableSize = 256;
        private const int TableMask = TableSize - 1;

        private readonly int[] _perm;

        public PerlinNoise(int seed)
        {
            _perm = BuildPermutation(seed);
        }

        public float Noise(float x, float y) => Remap(Gradient(x, y));

        // several octaves stacked, each finer and fainter than the last
        public float Fractal(float x, float y, int octaves, float persistence, float lacunarity)
        {
            if (octaves < 1) throw new ArgumentOutOfRangeException(nameof(octaves), "Must be at least 1.");

            float sum = 0f;
            float amplitude = 1f;
            float frequency = 1f;
            float total = 0f;
            for (int i = 0; i < octaves; i++)
            {
                sum += amplitude * Gradient(x * frequency, y * frequency);
                total += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            return Remap(sum / total);
        }

        // raw noise in roughly [-1, 1]
        private float Gradient(float x, float y)
        {
            int floorX = FloorToInt(x);
            int floorY = FloorToInt(y);
            int xi = floorX & TableMask;
            int yi = floorY & TableMask;
            float xf = x - floorX;
            float yf = y - floorY;

            float u = Fade(xf);
            float v = Fade(yf);

            int aa = _perm[_perm[xi] + yi];
            int ab = _perm[_perm[xi] + yi + 1];
            int ba = _perm[_perm[xi + 1] + yi];
            int bb = _perm[_perm[xi + 1] + yi + 1];

            float lower = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1f, yf), u);
            float upper = Lerp(Grad(ab, xf, yf - 1f), Grad(bb, xf - 1f, yf - 1f), u);
            return Lerp(lower, upper, v);
        }

        // one of eight directions picked by the low bits of the hash
        private static float Grad(int hash, float x, float y)
        {
            switch (hash & 7)
            {
                case 0: return x + y;
                case 1: return -x + y;
                case 2: return x - y;
                case 3: return -x - y;
                case 4: return x;
                case 5: return -x;
                case 6: return y;
                default: return -y;
            }
        }

        private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        private static float Lerp(float a, float b, float t) => a + t * (b - a);

        private static float Remap(float n)
        {
            float mapped = n * 0.5f + 0.5f;
            if (mapped < 0f) return 0f;
            if (mapped > 1f) return 1f;
            return mapped;
        }

        private static int FloorToInt(float value)
        {
            int i = (int)value;
            return value < i ? i - 1 : i;
        }

        private static int[] BuildPermutation(int seed)
        {
            var source = new int[TableSize];
            for (int i = 0; i < TableSize; i++) source[i] = i;

            var rng = new Random(seed);
            for (int i = TableSize - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (source[i], source[j]) = (source[j], source[i]);
            }

            var perm = new int[TableSize * 2];
            for (int i = 0; i < perm.Length; i++) perm[i] = source[i & TableMask];
            return perm;
        }
    }
}
