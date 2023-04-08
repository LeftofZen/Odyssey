using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Graph
{
	internal record struct Edge
	{
		public Edge(INode start, INode end, bool direction = true, float weight = 1f)
		{
			Start = start;
			End = end;
			Direction = direction;
			Weight = weight;
		}

		public INode Start { get; init; }
		public INode End { get; init; }

		public bool Direction { get; init; } // only for DAG
		public float Weight { get; init; } // only for weighted graphs
	}
}
