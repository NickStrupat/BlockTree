import { Queue } from "../Common/Queue";

export abstract class TreeNodeBase<T> {
	constructor(readonly value: T) { }
	private readonly _children = new Array<TreeNode<T>>();
	get children(): ReadonlyArray<TreeNode<T>> { return this._children; }

	addChildNode(value: T): TreeNode<T> {
		const node = new TreeNode<T>(this, value);
		this._children.push(node);
		return node;
	}

	print(valueToString?: (value: T) => string): void {
		this.printInternal('', true, valueToString);
	}

	private printInternal(indent: string, last: boolean, valueToString?: (value: T) => string): void {
		let line = indent;
		if (last) {
			line += '└─';
			indent += '  ';
		}
		else {
			line += '├─';
			indent += '│ ';
		}
		const text = valueToString === undefined ? this.value : valueToString(this.value);
		console.log(line + text);

		for (let i = 0; i !== this.children.length; i++)
			this.children[i].printInternal(indent, i === this.children.length - 1, valueToString);
	}

	* traverseDescendantsDepthFirst(): IterableIterator<NodeLevel<T>> {
		const stack = new Array<NodeLevel<T>>();
		for (let child of this.children)
			stack.push({ node: child, level: 0 });
		while (stack.length !== 0) {
			const next = stack.pop()!;
			yield next;
			for (let child of next.node.children)
				stack.push({ node: child, level: next.level + 1 });
		}
	}

	* traverseDescendantsBreadthFirst(): IterableIterator<NodeLevel<T>> {
		const queue = new Queue<NodeLevel<T>>(); // using array.shirt() as queue pop is slow O(N)
		for (let child of this.children)
			queue.push({ node: child, level: 0 });
		while (!queue.empty()) {
			const next = queue.pop()!;
			yield next;
			for (let child of next.node.children)
				queue.push({ node: child, level: next.level + 1 });
		}
	}
}

export type NodeLevel<T> = {
	node: TreeNodeBase<T>;
	level: number;
}

export class TreeRootNode<T> extends TreeNodeBase<T> {
}

export class TreeNode<T> extends TreeNodeBase<T> {
	constructor(
		readonly parent: TreeNodeBase<T>,
		value: T
	) {
		super(value);
	}
}