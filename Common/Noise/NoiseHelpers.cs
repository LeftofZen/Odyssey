namespace Odyssey.Noise
{
	public static class NoiseHelpers
	{
		public static double[,] NormaliseNoise2D(double[,] data)
		{
			var min = double.MaxValue;
			var max = double.MinValue;
			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					min = Math.Min(min, data[x, y]);
					max = Math.Max(max, data[x, y]);
				}
			}

			var range = max - min;
			var xx = 1f / range;

			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					data[x, y] = (data[x, y] - min) * xx;
				}
			}

			return data;
		}

		public static double[,] CreateNoise2D(NoiseParams noiseSettings)
		{
			var noise = new OpenSimplexNoise(noiseSettings.Seed);
			var data = new double[noiseSettings.NoiseSizeX, noiseSettings.NoiseSizeY];
			for (var y = 0; y < data.GetLength(1); y++)
			{
				for (var x = 0; x < data.GetLength(0); x++)
				{
					var amplitude = (double)noiseSettings.InitialAmplitude;
					var frequency = (double)noiseSettings.InitialFrequency;
					var totalAmplitude = 0.0;
					var total = 0.0;
					var xEval = x + noiseSettings.Offset.X;
					var yEval = y + noiseSettings.Offset.Y;

					for (var o = 0; o < noiseSettings.Octaves; o++)
					{
						var noisev = noise.Evaluate(xEval * frequency, yEval * frequency);

						// [[-1, 1] -> [0, 1]
						noisev = (noisev + 1) / 2;
						noisev *= amplitude;
						total += noisev;

						totalAmplitude += amplitude;
						amplitude *= noiseSettings.Persistence;
						frequency *= noiseSettings.Lacunarity;
					}

					//total = Math.Pow(total, total * 5);
					total = Math.Pow(total, noiseSettings.Redistribution);

					//terraces
					if (noiseSettings.UseTerracing)
					{
						total = Math.Round(total * noiseSettings.TerraceCount);
					}

					// normalise
					total /= totalAmplitude;

					data[x, y] = total;
				}
			}

			var dd = NormaliseNoise2D(data);
			//var dd = data;

			if (noiseSettings.UseKernel)
			{
				var identityKernel = new double[,] { { 1f } };
				var smoothingKernel = new double[,]
				{
					{ 1f, 1f, 1f },
					{ 1f, 1f, 1f },
					{ 1f, 1f, 1f },
				};

				var sharpenKernel = new double[,]
				{
					{ 0f, -1f, 0f },
					{ -1f, 5f, -1f },
					{ 0f, -1f, 0f },
				};

				var outlineKernel = new double[,]
				{
					{ -1f, -1f, -1f },
					{ -1f, 8f, -1f },
					{ -1f, -1f, -1f },
				};

				var topSobel = new double[,]
				{
					{ 1f, 2f, 1f },
					{ 0f, 0f, 0f },
					{ -1f, -2f, -1f },
				};
				dd = ApplyKernel(dd, smoothingKernel);
			}

			//var erosion = new Erosion();
			//var dmap = To1D(dd);
			//erosion.Erode(dmap, 256, 10, false);
			//dd = To2D(dmap);

			var res = AddBorder(dd);
			return res;
		}

		public static double[,] AddBorder(double[,] data, double borderValue = 0f)
		{
			// top, bottom
			for (var x = 0; x < data.GetLength(0); x++)
			{
				data[x, 0] = borderValue;
				data[x, data.GetLength(1) - 1] = borderValue;
			}

			// left, right
			for (var y = 0; y < data.GetLength(1); y++)
			{
				data[0, y] = borderValue;
				data[data.GetLength(0) - 1, y] = borderValue;
			}

			return data;
		}

		public static double[,] ApplyKernel(double[,] data, double[,] kernel)
		{
			var tmp = new double[data.GetLength(0), data.GetLength(1)];
			var kernelSize = kernel.GetLength(0) * kernel.GetLength(1);

			var halfsizeX = kernel.GetLength(0) / 2;
			var halfsizeY = kernel.GetLength(1) / 2;

			var kernelSpotsUsed = 0;
			for (var ky = 0; ky < kernel.GetLength(1); ky++)
			{
				for (var kx = 0; kx < kernel.GetLength(0); kx++)
				{
					if (kernel[kx, ky] != 0f)
					{
						kernelSpotsUsed++;
					}
				}
			}

			for (var y = halfsizeY; y < data.GetLength(1) - halfsizeY; y++)
			{
				for (var x = halfsizeX; x < data.GetLength(0) - halfsizeX; x++)
				{
					// apply kernel

					// even size kernel

					// odd size kernel
					var tmpres = 0.0;

					//kernel
					for (var ky = 0; ky < kernel.GetLength(1); ky++)
					{
						for (var kx = 0; kx < kernel.GetLength(0); kx++)
						{
							tmpres += data[x + kx - halfsizeX, y + ky - halfsizeY] * kernel[kx, ky];
						}
					}

					tmp[x, y] = tmpres;
				}
			}

			// can remove this normalise if we normalise tmpres above (requires getting min/max of kernel
			return NormaliseNoise2D(tmp);
		}
	}
}
