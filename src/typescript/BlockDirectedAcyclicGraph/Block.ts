import { sign } from 'tweetnacl';
import { Buffer } from "buffer";
import { InvalidParentSignaturesLengthError, BlockVerificationError, InvalidPublicKeyLengthError, InvalidSignatureLengthError } from './BlockErrors';

function areEqual(a: Buffer, b: Buffer): boolean {
	if (a.length !== b.length)
		return false;
	for (let i = 0; i !== a.length; i++)
		if (a[i] !== b[i])
			return false;
	return true;
}

const sizeofInt32 = 4;

export class Block {
	readonly publicKey: Buffer;
	readonly parentSignatures: Buffer;
	readonly data: Buffer;
	readonly signature: Buffer;
	readonly isVerified: Boolean;

	private constructor(
		publicKey: Buffer,
		parentSignatures: Buffer,
		data: Buffer,
		signature: Buffer,
	);

	private constructor(
		parentSignatures: Buffer,
		data: Buffer,
		keyPair: nacl.SignKeyPair,
	);

	private constructor(
		param1: Buffer,
		param2: Buffer,
		param3: Buffer | nacl.SignKeyPair,
		param4?: Buffer,
	) {
		if (param3 instanceof Buffer)
		{
			const publicKey = param1;
			const parentSignatures = param2;
			const data = param3;
			const signature = param4 as Buffer;

			this.publicKey = publicKey;
			this.parentSignatures = parentSignatures;
			this.data = data;
			this.signature = signature;
			this.isVerified = this.verify();
		}
		else
		{
			const parentSignatures = param1;
			const data = param2;
			const keyPair = param3;

			if (!Block.isParentSignaturesLengthValid(parentSignatures.length))
				throw new InvalidParentSignaturesLengthError();
			this.publicKey = Buffer.from(keyPair.publicKey);
			this.parentSignatures = parentSignatures;
			this.data = data;
			this.signature = Buffer.from(this.sign(keyPair));
			this.isVerified = true;
		}
	}

	static createSigned(
		parentSignatures: Buffer,
		data: Buffer,
		keyPair: nacl.SignKeyPair,
	): Block {
		return new Block(parentSignatures, data, keyPair);
	}

	*parentSignaturesIterable(): IterableIterator<Buffer> {
		if (this.parentSignatures.length === 0)
			return;
		for (let i = 0; i !== this.parentSignatures.length / sign.signatureLength; i++)
			yield this.parentSignatures.slice(i, i + sign.signatureLength);
	}

	private getBytesForCrypto(): Buffer {
		const bytesForCrypto = Buffer.alloc(this.parentSignatures.length + this.data.length);
		bytesForCrypto.set(this.parentSignatures);
		bytesForCrypto.set(this.data, this.parentSignatures.length);
		return bytesForCrypto;
	}

	private sign(keyPair: nacl.SignKeyPair): Buffer {
		const signedBytes = sign.detached(this.getBytesForCrypto(), keyPair.secretKey);
		return Buffer.from(signedBytes.buffer);
	}

	verify(): boolean {
		return sign.detached.verify(this.getBytesForCrypto(), this.signature, this.publicKey);
	}

	private static isParentSignaturesLengthValid(parentSignaturesLength: number): boolean {
		return parentSignaturesLength % sign.signatureLength === 0;
	}


	equals(other: Block): boolean {
		return ( // check lengths first because that's fast and rules out any obvious mismatches
			this.publicKey.byteLength === other.publicKey.byteLength &&
			this.parentSignatures.byteLength === other.parentSignatures.byteLength &&
			this.data.byteLength === other.data.byteLength &&
			this.signature.byteLength === other.signature.byteLength
		) &&
		(
			( // check if the arrays are actually the exact same instance (e.g. comparing to itself)
				this.publicKey === other.publicKey &&
				this.parentSignatures === other.parentSignatures &&
				this.data === other.data &&
				this.signature === other.signature
			) ||
			( // actually compare the contents of the arrays for equality
				areEqual(this.publicKey, other.publicKey) &&
				areEqual(this.parentSignatures, other.parentSignatures) &&
				areEqual(this.data, other.data) &&
				areEqual(this.signature, other.signature)
			)
		);
	}

	// serialization

	get serializationLength(): number {
		const sizeofInt32 = 4;
		return sizeofInt32 + this.publicKey.length +
			sizeofInt32 + this.parentSignatures.length +
			sizeofInt32 + this.data.length +
			sizeofInt32 + this.signature.length;
	}

	serialize(destination: Buffer): void {
		let buffer = destination.slice(0, this.serializationLength);

		buffer.writeInt32BE(this.publicKey.length, 0);
		buffer = buffer.slice(sizeofInt32);
		this.publicKey.copy(buffer, 0);
		buffer = buffer.slice(this.publicKey.length);

		buffer.writeInt32BE(this.parentSignatures.length, 0);
		buffer = buffer.slice(sizeofInt32);
		this.parentSignatures.copy(buffer, 0);
		buffer = buffer.slice(this.parentSignatures.length);

		buffer.writeInt32BE(this.data.length, 0);
		buffer = buffer.slice(sizeofInt32);
		this.data.copy(buffer, 0);
		buffer = buffer.slice(this.data.length);

		buffer.writeInt32BE(this.signature.length, 0);
		buffer = buffer.slice(sizeofInt32);
		this.signature.copy(buffer, 0);
	}

	private static deserializeInternal(rawBlockBytes: Buffer): Block | Error {
		let buffer = rawBlockBytes;

		const publicKeyLength = buffer.readInt32BE(0);
		if (publicKeyLength !== sign.publicKeyLength)
			return new InvalidPublicKeyLengthError();
		buffer = buffer.slice(sizeofInt32);
		const publicKey = buffer.slice(0, publicKeyLength);
		buffer = buffer.slice(publicKeyLength);

		const parentSignaturesLength = buffer.readInt32BE(0);
		if (!Block.isParentSignaturesLengthValid(parentSignaturesLength))
			return new InvalidParentSignaturesLengthError();
		buffer = buffer.slice(sizeofInt32);
		const parentSignatures = buffer.slice(0, parentSignaturesLength);
		buffer = buffer.slice(parentSignaturesLength);

		const dataLength = buffer.readInt32BE(0);
		buffer = buffer.slice(sizeofInt32);
		const data = buffer.slice(0, dataLength);
		buffer = buffer.slice(dataLength);

		const signatureLength = buffer.readInt32BE(0);
		if (signatureLength !== sign.signatureLength)
			return new InvalidSignatureLengthError();
		buffer = buffer.slice(sizeofInt32);
		const signature = buffer.slice(0, signatureLength);
		buffer = buffer.slice(signatureLength);

		const unverifiedBlock = new Block(publicKey, parentSignatures, data, signature);
		if (!unverifiedBlock.isVerified)
			return new BlockVerificationError();
		
		return unverifiedBlock;
	}

	static tryDeserialize(rawBlockBytes: Buffer): Block | undefined {
		const result = Block.deserializeInternal(rawBlockBytes);
		if (result instanceof Error)
			throw undefined;
		return result;
	}

	static deserialize(rawBlockBytes: Buffer): Block {
		const result = Block.deserializeInternal(rawBlockBytes);
		if (result instanceof Error)
			throw result;
		return result;
	}
}