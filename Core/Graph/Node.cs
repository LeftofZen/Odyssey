namespace Odyssey.Graph
{
	interface INode
	{
		ICollection<Edge> Edges { get; }
	}

	internal class Node<T> : INode
	{
		public ICollection<Edge> Edges { get; }
		public T Datum { get; }

		public Node(T datum)
		{
			Datum = datum;
		}
	}
}
