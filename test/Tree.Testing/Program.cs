using System;
using System.Linq;

namespace Tree.Testing
{
	class Program
	{
		static void Main(string[] args)
		{
			var tree = new Tree<String>("A0");
			var b = tree.Root.AddChildNode("B0");
			
			var c0 = b.AddChildNode("C0");
			var d0 = c0.AddChildNode("D0");
			var d1 = c0.AddChildNode("D1");

			var c1 = b.AddChildNode("C1");
			var e0 = c1.AddChildNode("E0");
			var e1 = c1.AddChildNode("E1");

			tree.Root.Print(Console.Out);
			
			var nodesDepthFirst = tree.TraverseDepthFirst().ToList();
			foreach (var node in nodesDepthFirst)
				Console.WriteLine(new String(' ', (Int32)node.Level * 2) + node.Node);
				
			var nodesBreadthFirst = tree.TraverseBreadthFirst().ToList();
			foreach (var node in nodesBreadthFirst)
				Console.WriteLine(new String(' ', (Int32)node.Level * 2) + node.Node);
		}
	}
}
