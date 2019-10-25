import { TreeRootNode, NodeLevel } from "./TreeNode";

export class Tree<T> {
	readonly root: TreeRootNode<T>;
	
	constructor(rootValue: T) {
		this.root = new TreeRootNode<T>(rootValue);
	}

	* traverseDepthFirst(): IterableIterator<NodeLevel<T>> {
		yield { node: this.root, level: 0 };
		for (let nodeLevel of this.root.traverseDescendantsDepthFirst())
			yield  { node: nodeLevel.node, level: nodeLevel.level + 1 };
	}

	* traverseBreadthFirst(): IterableIterator<NodeLevel<T>> {
		yield { node: this.root, level: 0 };
		for (let nodeLevel of this.root.traverseDescendantsBreadthFirst())
			yield  { node: nodeLevel.node, level: nodeLevel.level + 1 };
	}
}