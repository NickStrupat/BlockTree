import { Buffer } from "buffer";

export class Convert {
	private constructor() {}

	static fromBase64String(s: string): Uint8Array {
		return Buffer.from(s, 'base64');
	}

	static toBase64String(bytes: Uint8Array): string {
		return Buffer.from(bytes).toString('base64');
	}

	static fromHex(s: string): Uint8Array {
		return Buffer.from(s, 'hex');
	}

	static toHex(bytes: Uint8Array): string {
		return Buffer.from(bytes).toString('hex');
	}
}