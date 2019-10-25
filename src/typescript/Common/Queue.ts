 // using array.shirt() as queue pop is slow O(N)

type Node<T> = {
	item: T;
	next: Node<T> | null;
}

export class Queue<T>
{
	first: Node<T> | null = null;
	last: Node<T> | null = null;

	push(item: T): void {
		if (this.empty()) {
			this.first = { item, next: null };
			this.last = this.first;
		}
		else {
			this.last!.next = { item, next: null };
			this.last = this.last!.next;
		}
	}

	pop(): T {
		if (this.first === null)
			throw "No elements";
		if (this.last === this.first)
			this.last = null;
		const item = this.first.item;
		this.first = this.first.next;
		return item;
	}

	empty(): boolean {
		return this.first === null;
	}
}