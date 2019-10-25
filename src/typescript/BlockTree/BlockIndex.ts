import { Block } from "./Block";

export class BlockIndex {
	private readonly blocksBySignature = new Map<string, Block>();
	private readonly childBlocksByParentSignature = new Map<string, Map<string, Block>>();

	private static readonly textDecoder = new TextDecoder();
	private static getString(bytes: Uint8Array): string {
		return BlockIndex.textDecoder.decode(bytes);
	}

	contains(signature: Uint8Array): boolean {
		const key = BlockIndex.getString(signature);
		return this.blocksBySignature.get(key) !== undefined;
	}

	tryGetBySignature(signature: Uint8Array): Block | undefined {
		const key = BlockIndex.getString(signature);
		return this.blocksBySignature.get(key);
	}

	tryGetChildren(signature: Uint8Array): readonly Block[] | undefined {
		const key = BlockIndex.getString(signature);
		const set = this.childBlocksByParentSignature.get(key);
		if (set === undefined)
			return undefined;
		return Array.from(set.values());
	}

	add(block: Block): void {
		const key = BlockIndex.getString(block.signature);
		if (this.blocksBySignature.has(key))
			throw "Block with this signature already added";
		this.blocksBySignature.set(key, block);

		const parentKey = BlockIndex.getString(block.parentSignature);
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
}