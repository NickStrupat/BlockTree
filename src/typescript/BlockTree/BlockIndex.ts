import { Block } from "./Block";
import { Convert } from "./Convert";
import { NodeLevel, Traverse } from "./Traverse";

export class BlockIndex {
	private readonly blocksBySignature = new Map<string, Block>();
	private readonly childBlocksByParentSignature = new Map<string, Map<string, Block>>();

	contains(signature: Uint8Array): boolean {
		const key = Convert.toBase64String(signature);
		return this.blocksBySignature.get(key) !== undefined;
	}

	tryGetBySignature(signature: Uint8Array): Block | undefined {
		const key = Convert.toBase64String(signature);
		return this.blocksBySignature.get(key);
	}

	tryGetChildren(signature: Uint8Array): readonly Block[] | undefined {
		const key = Convert.toBase64String(signature);
		const set = this.childBlocksByParentSignature.get(key);
		if (set === undefined)
			return undefined;
		return Array.from(set.values());
	}

	add(block: Block): void {
		const key = Convert.toBase64String(block.signature);
		if (this.blocksBySignature.has(key))
			throw "Block with this signature already added";
		this.blocksBySignature.set(key, block);

		const parentKey = Convert.toBase64String(block.parentSignature);
		const childBlocks = this.childBlocksByParentSignature.get(parentKey);
		if (childBlocks === undefined)
			this.childBlocksByParentSignature.set(parentKey, new Map<string, Block>([[key, block]]));
		else
		{
			if (childBlocks.has(key)) {
				this.blocksBySignature.delete(key);
				throw "Block with this parent signature already added";
			}
			childBlocks.set(key, block);
		}
	}

	getAllBlocks(): readonly Block[] {
		return Array.from(this.blocksBySignature.values());
	}

	private getChildren(node: Block): IterableIterator<Block> {
		const children = this.tryGetChildren(node.signature);
		if (children === undefined)
			return [].values();
		return children.values();
	}

	traverseDepthFirst(root: Block): IterableIterator<NodeLevel<Block>> {
		return Traverse.depthFirst(root, this.getChildren);
	}

	traverseBreadthFirst(root: Block): IterableIterator<NodeLevel<Block>> {
		return Traverse.breadthFirst(root, this.getChildren);
	}
}