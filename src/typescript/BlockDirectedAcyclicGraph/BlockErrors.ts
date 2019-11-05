export abstract class BlockError extends Error {
	get [Symbol.toStringTag]() { return "BlockVerificationError" }
}

export class BlockVerificationError extends BlockError {
	get [Symbol.toStringTag]() { return "BlockVerificationError" }
}

export class InvalidPublicKeyLengthError extends BlockError {
	get [Symbol.toStringTag]() { return "InvalidPublicKeyLengthError" }
}

export class InvalidParentSignaturesLengthError extends BlockError {
	get [Symbol.toStringTag]() { return "InvalidParentSignaturesLengthError" }
}

export class InvalidSignatureLengthError extends BlockError {
	get [Symbol.toStringTag]() { return "InvalidSignatureLengthError" }
}