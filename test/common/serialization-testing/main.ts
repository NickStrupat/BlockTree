import { writeFileSync, readFileSync } from 'fs';
import { Block } from "../../../src/typescript/BlockDirectedAcyclicGraph/Block";

const bytes = readFileSync('../SerializationTesting/block.bin');
const block = Block.deserialize(bytes);
console.log(block.isVerified);
console.log(block.verify());
console.log(block);