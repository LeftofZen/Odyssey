using Microsoft.Xna.Framework;

namespace Odyssey.Noise
{
	public record class NoiseParams
	{
		public int Seed = 0;
		public int Octaves = 8;
		public float Lacunarity = 3f;
		public float Persistence = 0.5f;
		public float InitialAmplitude = 1f;
		public float InitialFrequency = 0.005f;
		public int NoiseSizeX = 256;
		public int NoiseSizeY = 256;
		public Vector2 Offset = Vector2.Zero;
		public bool UseKernel = false;
		public float Redistribution = 1f;
		public bool UseTerracing = false;
		public int TerraceCount = 10;
	}
}
