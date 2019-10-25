import { Tree } from "./Tree";

const tree = new Tree<string>('root');

const a = tree.root.addChildNode('a');
a.addChildNode('1');
a.addChildNode('2');

const b = tree.root.addChildNode('b');
b.addChildNode('!');
b.addChildNode('@');

tree.root.print();