using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Odyssey.Graph
{
	internal class Graph
	{
		ICollection<INode> Nodes { get; }
		ICollection<Edge> Edges { get; }

		public Edge Connect(INode a, INode b)
		{
			var edge = new Edge(a, b);
			if (!a.Edges.Contains(edge)) a.Edges.Add(edge);
			if (!b.Edges.Contains(edge)) b.Edges.Add(edge);
			if (!Edges.Contains(edge)) Edges.Add(edge);
			return edge;
		}

		public void Disconnect(INode a, INode b)
		{
			var edge = new Edge(a, b);
			var edge2 = new Edge(b, a);

			a.Edges.Remove(edge);
			a.Edges.Remove(edge2);
			b.Edges.Remove(edge);
			b.Edges.Remove(edge2);
			Edges.Remove(edge);
			Edges.Remove(edge2);
		}

		public LinkedList<INode>ShortestPath(INode a, INode b)
		{
			var path = new LinkedList<INode>();
			path.AddLast(a);
			path.AddLast(b);
		}
	}
}
