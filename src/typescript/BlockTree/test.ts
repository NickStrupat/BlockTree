import { BlockTree } from "./BlockTree";
import nacl from "tweetnacl";
import { Buffer } from "buffer";

const bobKey = nacl.sign.keyPair();
const aliceKey = nacl.sign.keyPair();
const blockTree = new BlockTree(Buffer.from("hello"), bobKey);
const aliceReply = blockTree.tryAdd(blockTree.root, Buffer.from("hi"), aliceKey);
// blockTree.root.print(x => Buffer.from(x.data).toString('utf8'));
console.log(blockTree.root.asStrings());
console.log(aliceReply!.asStrings());