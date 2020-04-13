using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonogameTest1
{
	class NoiseParams
	{
		public int Octaves = 8;
		public float Lacunarity = 3f;
		public float Persistence = 0.5f;
		public float InitialAmplitude = 1f;
		public float InitialFrequency = 0.005f;
		public int NoiseSize = 512;
		public Vector2 Offset = Vector2.Zero;

		public bool IsEqualTo(NoiseParams ns)
		{
			return Octaves == ns.Octaves
				&& Lacunarity == ns.Lacunarity
				&& Persistence == ns.Persistence
				&& InitialAmplitude == ns.InitialAmplitude
				&& InitialFrequency == ns.InitialFrequency
				&& NoiseSize == ns.NoiseSize
				&& Offset == ns.Offset;
		}

		public void Set(NoiseParams ns)
		{
			Octaves = ns.Octaves;
			Lacunarity = ns.Lacunarity;
			Persistence = ns.Persistence;
			InitialAmplitude = ns.InitialAmplitude;
			InitialFrequency = ns.InitialFrequency;
			NoiseSize = ns.NoiseSize;
			Offset = ns.Offset;
		}
	}
}
