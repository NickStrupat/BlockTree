import { sign } from 'tweetnacl';
import { Convert } from './Convert';

function areEqual(a: Uint8Array, b: Uint8Array): boolean {
	if (a.length !== b.length)
		return false
	for (let i = 0; i !== a.length; i++)
		if (a[i] !== b[i])
			return false;
	return true;
}

type NonFunctionPropertyNames<T> = { [K in keyof T]: T[K] extends Function ? never : K }[keyof T];
type NonFunctionPropertiesAsStrings<T> = { [P in NonFunctionPropertyNames<T>]: string };

export class Block {
	readonly parentSignature: Uint8Array;
	readonly data: Uint8Array;
	readonly signature: Uint8Array;
	readonly publicKey: Uint8Array;

	constructor(
		parentSignature: Uint8Array,
		data: Uint8Array,
		keyPair: nacl.SignKeyPair,
	) {
		this.parentSignature = parentSignature;
		this.data = data;
		this.signature = this.sign(keyPair);
		this.publicKey = keyPair.publicKey;
	}

	private getBytesForCrypto(): Uint8Array {
		const bytesForCrypto = new Uint8Array(this.parentSignature.length + this.data.length);
		bytesForCrypto.set(this.parentSignature);
		bytesForCrypto.set(this.data, this.parentSignature.length);
		return bytesForCrypto;
	}

	private sign(keyPair: nacl.SignKeyPair): Uint8Array {
		return sign.detached(this.getBytesForCrypto(), keyPair.secretKey);
	}

	private verify(): boolean {
		return sign.detached.verify(this.getBytesForCrypto(), this.signature, this.publicKey);
	}

	verifyChild(child: Block): boolean {
		// simple check if signatures match
		if (!areEqual(this.signature, child.parentSignature))
			return false;
		// actual data integrity check that the signature of (parent signature + data) results in the child's signature
		return child.verify();
	}

	toString = () : string => {
		return Convert.toBase64String(this.data);
	}

	asStrings(): NonFunctionPropertiesAsStrings<Block> {
		return {
			parentSignature: Convert.toBase64String(this.parentSignature),
			data: Convert.toBase64String(this.data),
			signature: Convert.toBase64String(this.signature),
			publicKey: Convert.toBase64String(this.publicKey),
		};
	}
}