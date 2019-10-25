import { BlockTree } from "./BlockTree";
import nacl from "tweetnacl";
import { Buffer } from "buffer";

const bobKey = nacl.sign.keyPair();
const aliceKey = nacl.sign.keyPair();
const blockTree = new BlockTree(Buffer.from("hello", "utf16le"), bobKey);
const aliceReply = blockTree.tryAdd(blockTree.root, Buffer.from("hi", "utf16le"), aliceKey);
blockTree.root.print();