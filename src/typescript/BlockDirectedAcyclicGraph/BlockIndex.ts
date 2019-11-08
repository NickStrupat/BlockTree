import { Block } from "./Block";
import { Convert } from "../Common/Convert";
import { NodeLevel, Traverse } from "../Common/Traverse";
import { Buffer } from "buffer";

function tryAddValue(map: Map<string, Map<string, Block>>, key: string, block: Block): boolean {
	const values = map.get(key);
	const key2 = Convert.toBase64String(block.parentSignatures)
	if (values !== undefined) {
		if (values.has(key2))
			return false;
		values.set(key2, block);
		return true;
	}
	const map2 = new Map<string, Block>();
	map2.set(key2, block);
	map.set(key, map2);
	return true;
}

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
		try {
			if (this.blocksBySignature.has(key))
				throw new Error("An element with the same key already exists in the set");
			this.blocksBySignature.set(key, block);

			const addOrThrow = (parentSignature: Buffer, block: Block): void => {
				const psk = Convert.toBase64String(parentSignature);
				if (!tryAddValue(this.childBlocksByParentSignature, psk, block))
					throw new Error("An element with the same key already exists in the set");
			};

			if (block.parentSignatures.length == 0)
				addOrThrow(block.parentSignatures, block);
			else
				for (let parentSignature of block.parentSignaturesIterable())
					addOrThrow(parentSignature, block);
		}
		catch (err) {
			this.blocksBySignature.delete(key);

			const reomveIfExists = (parentSignature: Buffer, block: Block): void => {
				const psk = Convert.toBase64String(parentSignature);
				const childBlocks = this.childBlocksByParentSignature.get(psk);
				if (childBlocks !== undefined) {
					const key2 = Convert.toBase64String(block.parentSignatures)
					if (childBlocks.delete(key2) && childBlocks.size === 0)
						this.childBlocksByParentSignature.delete(psk)
				}
			};

			if (block.parentSignatures.length == 0)
				reomveIfExists(block.parentSignatures, block);
			else
				for (let parentSignature of block.parentSignaturesIterable())
					reomveIfExists(parentSignature, block);

			throw err;
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