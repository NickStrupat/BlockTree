import { BlockIndex } from "./BlockIndex";
import { Block } from "./Block";
import { Tree } from "../Tree/Tree";
import { TreeNodeBase, TreeRootNode } from "../Tree/TreeNode";
import { Queue } from "../Common/Queue";

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
	private readonly blockIndex = new BlockIndex();
	private readonly tree: Tree<Block>;

	private static readonly emptyBytes = new Uint8Array(0);

	get root(): TreeRootNode<Block> {
		return this.tree.root;
	}

	constructor(unverifiedBlocks: IterableIterator<Block>);
	constructor(rootData: Uint8Array, keyPair: nacl.SignKeyPair);

	constructor(param1: IterableIterator<Block> | Uint8Array, param2?: nacl.SignKeyPair) {
		if (param2 !== undefined) {
			const rootData = param1 as Uint8Array;
			const keyPair = param2;

			const rootBlock = new Block(BlockTree.emptyBytes, rootData, keyPair);
			this.blockIndex = new BlockIndex();
			this.blockIndex.add(rootBlock);
			this.tree = new Tree<Block>(rootBlock);
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
			const rootBlock = maybeRootBlock[0];

			// verify the blocks and build a tree
			this.tree = new Tree<Block>(rootBlock);
			const queue = new Queue<TreeNodeBase<Block>>(); // using array.shirt() as queue pop is slow O(N)
			queue.push(this.tree.root);
			while (!queue.empty()) {
				const current = queue.pop();
				const children = unverifiedBlockIndex.tryGetChildren(current.value.signature);
				if (children === undefined)
					continue;
				for (let child of children) {
					if (current.value.verifyChild(child))
						queue.push(current.addChildNode(child))
					else
						throw new InvalidBlocksError(current.value, child);
				}
			}

			this.blockIndex = unverifiedBlockIndex; // now verified
		}
	}

	tryAdd(parent: TreeNodeBase<Block>, data: Uint8Array, key: nacl.SignKeyPair): TreeNodeBase<Block> | undefined {
		if (!this.blockIndex.contains(parent.value.signature))
			return undefined;

		const childBlock = new Block(parent.value.signature, data, key);
		this.blockIndex.add(childBlock);
		return parent.addChildNode(childBlock);
	}

	getAllBlocks(): readonly Block[] {
		return this.blockIndex.getAllBlocks();
	}
}