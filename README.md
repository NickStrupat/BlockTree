# BlockTree
Blockchain lowest layer of just crypto-signed nodes forming an n-ary tree with verification. Consensus not included.


Notes

- An instance of a block is valid (construction/deserialization throws if any input data is invalid)
- a block index is only for fast look-up of blocks by their signature or by their parent's signature