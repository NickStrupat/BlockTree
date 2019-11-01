import { BlockIndex } from "./BlockIndex";
import { Block } from "./Block";
import { Queue } from "../Common/Queue";
import { NodeLevel } from "./Traverse";

export class NoRootBlockError extends Error {}

export class InvalidBlocksError extends Error {
	constructor(
		readonly parent: Block,
		readonly child: Block,
	) {
		super();
	}
}

export class BlockTree {
	public readonly root: Block;
	private readonly blockIndex = new BlockIndex();

	private static readonly emptyBytes = new Uint8Array(0);

	constructor(unverifiedBlocks: IterableIterator<Block>);
	constructor(rootData: Uint8Array, keyPair: nacl.SignKeyPair);

	constructor(param1: IterableIterator<Block> | Uint8Array, param2?: nacl.SignKeyPair) {
		if (param2 !== undefined) {
			const rootData = param1 as Uint8Array;
			const keyPair = param2;

			this.root = new Block(BlockTree.emptyBytes, rootData, keyPair);
			this.blockIndex = new BlockIndex();
			this.blockIndex.add(this.root);
		}
		else {
			const unverifiedBlocks = param1 as IterableIterator<Block>;

			// build up index of all blocks
			const unverifiedBlockIndex = new BlockIndex();
			for (let unverifiedBlock of unverifiedBlocks)
				unverifiedBlockIndex.add(unverifiedBlock);

			// get the root node
			const maybeRootBlock = unverifiedBlockIndex.tryGetChildren(BlockTree.emptyBytes);
			if (maybeRootBlock === undefined || maybeRootBlock.length !== 1)
				throw new NoRootBlockError();
			this.root = maybeRootBlock[0];

			// verify the blocks and build a tree
			const queue = new Queue<Block>(); // using array.shirt() as queue pop is slow O(N)
			queue.push(this.root);
			while (!queue.empty()) {
				const current = queue.pop();
				const children = unverifiedBlockIndex.tryGetChildren(current.signature);
				if (children === undefined)
					continue;
				for (let child of children) {
					if (current.verifyChild(child))
						queue.push(child)
					else
						throw new InvalidBlocksError(current, child);
				}
			}

			this.blockIndex = unverifiedBlockIndex; // now verified
		}
	}

	tryAdd(parent: Block, data: Uint8Array, key: nacl.SignKeyPair): Block | undefined {
		if (!this.blockIndex.contains(parent.signature))
			return undefined;

		const childBlock = new Block(parent.signature, data, key);
		this.blockIndex.add(childBlock);
		return childBlock;
	}

	getAllBlocks(): readonly Block[] {
		return this.blockIndex.getAllBlocks();
	}

	traverseDepthFirst(): IterableIterator<NodeLevel<Block>> {
		return this.blockIndex.traverseDepthFirst(this.root);
	}

	traverseBreadthFirst(): IterableIterator<NodeLevel<Block>> {
		return this.blockIndex.traverseBreadthFirst(this.root);
	}
}