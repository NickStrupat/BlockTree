import { Queue } from "../Common/Queue";

export type NodeLevel<T> = {
	node: T;
	level: number;
}

export class Traverse {
	private constructor() {}

	static * depthFirst<T>(root: T, childSelector: (node: T) => IterableIterator<T>): IterableIterator<NodeLevel<T>> {
		const stack = new Array<NodeLevel<T>>();
		stack.push({ node: root, level: 0 });
		while (stack.length !== 0) {
			const next = stack.pop()!;
			yield next;
			for (let child of childSelector(next.node))
				stack.push({ node: child, level: next.level + 1 });
		}
	}

	static * breadthFirst<T>(root: T, childSelector: (node: T) => IterableIterator<T>): IterableIterator<NodeLevel<T>> {
		const queue = new Queue<NodeLevel<T>>();
		queue.push({ node: root, level: 0 });
		while (!queue.empty()) {
			const next = queue.pop()!;
			yield next;
			for (let child of childSelector(next.node))
				queue.push({ node: child, level: next.level + 1 });
		}
	}
}