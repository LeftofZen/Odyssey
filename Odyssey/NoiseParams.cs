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
		public int Seed = 0;
		public int Octaves = 8;
		public float Lacunarity = 3f;
		public float Persistence = 0.5f;
		public float InitialAmplitude = 1f;
		public float InitialFrequency = 0.005f;
		public int NoiseSize = 256;
		public Vector2 Offset = Vector2.Zero;
		public bool UseKernel = false;
		public float Redistribution = 1f;
		public bool UseTerracing = false;
		public int TerraceCount = 10;

		public bool IsEqualTo(NoiseParams ns)
		{
			return Seed == ns.Seed
				&& Octaves == ns.Octaves
				&& Lacunarity == ns.Lacunarity
				&& Persistence == ns.Persistence
				&& InitialAmplitude == ns.InitialAmplitude
				&& InitialFrequency == ns.InitialFrequency
				&& NoiseSize == ns.NoiseSize
				&& Offset == ns.Offset
				&& UseKernel == ns.UseKernel
				&& Redistribution == ns.Redistribution
				&& UseTerracing == ns.UseTerracing
				&& TerraceCount == ns.TerraceCount;
		}

		public void Set(NoiseParams ns)
		{
			Seed = ns.Seed;
			Octaves = ns.Octaves;
			Lacunarity = ns.Lacunarity;
			Persistence = ns.Persistence;
			InitialAmplitude = ns.InitialAmplitude;
			InitialFrequency = ns.InitialFrequency;
			NoiseSize = ns.NoiseSize;
			Offset = ns.Offset;
			UseKernel = ns.UseKernel;
			Redistribution = ns.Redistribution;
			UseTerracing = ns.UseTerracing;
			TerraceCount = ns.TerraceCount;
		}
	}
}
