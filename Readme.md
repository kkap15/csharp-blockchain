# MiniChain

A blockchain built from scratch in C# / .NET 10 — for learning, not production.

Most "build a blockchain" tutorials are in Python or JavaScript. This one's in C#, partly because it's a language I work in daily and partly because there's surprisingly little material out there for the .NET crowd. The goal is to understand the primitives — hashing, linked blocks, proof-of-work, signed transactions, P2P consensus — by building each one by hand rather than reading about them.

Each milestone ships something working and demoable. The current state of the chain is reflected in the table below.

| Milestone | Status |
|---|---|
| Blocks, hashing, tamper detection | ✅ |
| Proof-of-Work mining | ✅ |
| Wallets & signed transactions (ECDSA) | ✅ |
| Mempool & block assembly | ✅ |
| P2P networking & longest-chain rule | ✅ |
| CLI wallet & polish | ✅ |
| SQLite persistence & Blazor frontend | ✅ |
| Merkle trees | ✅ |

## Quickstart

```bash
git clone https://github.com/<your-username>/minichain.git
cd minichain
dotnet restore
dotnet test
dotnet run --project MiniChain.Cli
```

You will be dropped into an interactive CLI. Run `wallet new` to generate a wallet, `mine` to earn your first coins, and `balance` to see your balance. Type `quit` to exit.

To run the Blazor web UI instead:

```bash
dotnet run --project MiniChain.Web
```

Open the URL shown in the terminal. Create a wallet, mine blocks, send transactions, and explore the chain — all from the browser. Works on iOS Safari too.

## What's in each milestone

### Blocks, hashing, tamper detection
- **`Block`** — index, timestamp, previous hash, transactions, nonce. Immutable except for the nonce (which the miner mutates during mining).
- **Deterministic hashing** — every block produces the same hash on every machine, every runtime, every locale. This sounds trivial but is the source of most "why don't my nodes agree" bugs in real chains.
- **`Blockchain`** — an ordered list of blocks with `IsValid()` that detects tampered transactions, broken hash links, and tampered genesis blocks.

### Proof-of-Work mining
- **`Miner`** — brute-forces a `Nonce` until the block's SHA-256 hash starts with `difficulty` leading hex zeros. Each hex zero = 4 bits of work, so difficulty 4 ≈ 1-in-65536 chance per attempt.
- **`Blockchain(difficulty)`** — mines every block on `AddBlock` and rejects unmined blocks in `IsValid()`.
- **`IMiner`** — injected into `Blockchain` so tests can swap in a zero-difficulty miner without waiting for real PoW.

### Wallets & signed transactions (ECDSA)
- **`Wallet`** — generates a P-256 ECDSA key pair. `Sign(data)` produces a hex signature; `PublicKeyHex` is the DER-encoded public key as hex.
- **`Transaction`** — `From`, `To`, `Amount`, `Signature`. `Sign(wallet)` attaches the sender's signature; `IsValid()` verifies it using the public key in `From` without needing the wallet instance.
- **`Blockchain.IsValid()`** — extended to reject blocks containing any transaction with an invalid or missing signature.
- `CultureInfo.InvariantCulture` used throughout the signable payload so amounts hash identically on every locale.

### Mempool & block assembly
- **`Mempool`** — a waiting room for broadcast transactions. `Submit` validates the signature before accepting; `Take(n)` returns the next N pending txs; `Remove` evicts confirmed ones after a block is mined.
- **`Blockchain.MineFromMempool(mempool, count)`** — pulls from the mempool, mines a block, and cleans up confirmed transactions in one call.

### P2P networking & longest-chain rule
- **`Node`** — wraps a `Blockchain` and holds a list of peer nodes. `Connect(peer)` links nodes; `Broadcast(block)` pushes your chain to all peers after mining; `AcceptChain(chain)` adopts an incoming chain if it is longer and passes `IsValidChain`.
- **`Blockchain.IsValidChain(chain)`** — validates an external list of blocks using this blockchain's own difficulty and miner, without mutating state.
- **`Blockchain.ReplaceChain(chain)`** — atomically replaces the internal block list (clear + refill).
- Longest-chain rule: a node only accepts a peer's chain if it is strictly longer and valid — the same consensus rule Bitcoin uses.

### CLI wallet & polish
- **Wallet persistence** — `ExportPrivateKey()` serializes the P-256 private key to hex; `SaveWallet(path)` writes it to a `WalletFile` JSON record; `LoadWallet(path)` reconstructs the full key pair. Identity survives restarts.
- **UTXO balance tracking** — `Blockchain.GetBalance(publicKeyHex)` walks every confirmed transaction and computes the net balance. Coinbase transactions (50 coins per block, `From = 0x00…00`) fund the miner and are excluded from the sender deduction path.
- **Balance-aware mempool** — `Mempool.Submit(tx, blockchain)` rejects transactions where the sender's confirmed balance is less than the amount. Coinbase transactions bypass this check. `Transaction.IsValid()` remains stateless (signature only).
- **Interactive CLI** — replaces the hardcoded demo with a command loop: `wallet new`, `wallet load`, `balance`, `send <to> <amount>`, `mine`, `chain`, `quit`.

### SQLite persistence & Blazor frontend
- **SQLite persistence** — blocks are saved to `minichain.db` (via EF Core) after every mine and reloaded on startup. `ChainRepository` maps between domain objects (`Block`, `Transaction`) and EF entities (`BlockEntity`, `TransactionEntity`). The chain survives server restarts.
- **Wallet auto-load** — `wallet.json` is loaded automatically on startup so the active wallet also survives restarts.
- **Blazor Server frontend** — four pages: Dashboard (chain stats + mine button), Chain Explorer (all blocks and transactions), Wallet (create, balance, send), Mempool (pending transactions + mine). All pages share live state via `ChainService` and update in real time via SignalR.
- **Async mining** — PoW runs on a thread pool thread so the UI stays responsive during mining. A "Mining..." button state gives visual feedback.
- **`ChainService`** implements `IHostedService` so chain and wallet are loaded before the first page renders.

### Merkle trees
- **`MerkleTree.ComputeRoot(transactions)`** — builds a binary hash tree from the transaction list. Each leaf is `SHA256(tx.SignablePayload())`; pairs are combined upward with `SHA256(left + right)`; odd layers duplicate the last node. An empty list returns a 64-zero sentinel.
- **`Block.MerkleRoot`** — computed property on every block, derived on demand from its transactions. Not stored in the database; always recalculated from the transaction data.
- **`Block.ComputeHash()`** — now commits to the Merkle root instead of the flat transaction concatenation. Changing any single transaction changes the root, which changes the block hash, which breaks the chain — the same tamper-detection property as before but with a structure that can produce compact inclusion proofs.

## Project layout

```
MiniChain.Core/
  Interface/   # IBlock, IBlockchain, ITransaction, IWallet, IMiner, IMempool, INode
  Models/      # WalletFile
  Services/    # Block, Blockchain, Miner, Wallet, Mempool, Node
  Utilities/   # HashingUtils, MerkleTree
MiniChain.Cli/     # Interactive CLI (wallet new/load, balance, send, mine, chain)
MiniChain.Web/     # Blazor Server frontend (Dashboard, Chain, Wallet, Mempool pages)
MiniChain.Tests/   # Unit tests
```

## Design notes

**Canonical pipe-delimited serialization for hashing, not JSON.** JSON property ordering isn't guaranteed across runtimes, and a single whitespace difference changes the hash. Two nodes hashing the same logical block must produce byte-identical strings. Bitcoin's serialization is binary and equally strict for the same reason.

**No `Hash` field on `Block`.** The hash is computed on demand from the data, so it can't drift out of sync with the data. If `Hash` were stored as a field, tampering with the data while leaving the field alone would defeat the entire chain.

**`MerkleRoot` is computed, never stored.** It is derived from the transaction list on demand, so it can't drift out of sync. The database persists only the raw transactions; `MerkleRoot` and `ComputeHash()` recalculate from those on load — the same approach as the block hash itself.

**Genesis is hard-coded with a fixed Unix timestamp.** Every node must produce the same genesis block. `DateTimeOffset.UtcNow` would give a different value on every node, and they could never agree on the chain's starting point. Bitcoin hard-codes its genesis too — that's why the embedded "Chancellor on brink…" headline is famous.

**`Transaction.IsValid()` verifies without the wallet.** The public key is embedded in `From` as a DER-encoded hex string. Any node can verify any transaction without storing key material — the same property that makes Bitcoin's UTXO model work.

**`IMiner` is injected into `Blockchain`.** This lets tests pass a zero-difficulty miner to skip PoW entirely, keeping the test suite fast without compromising the real mining logic.

**Numeric formatting uses `CultureInfo.InvariantCulture` everywhere.** Default `ToString()` on numbers respects the system locale — `1234.5` becomes `"1234,5"` on a German machine. That's a hash-breaker waiting to happen. Invariant culture sidesteps the entire problem.

## What's deliberately not in scope

- Smart contracts and a VM (large enough to be its own project)
- Merkle inclusion proofs (the tree is built; the proof path API is a natural next step)
- Proof-of-Stake (interesting but doubles the project)
- Persistent peer discovery and NAT traversal (localhost suffices)

## Tests

```bash
dotnet test
```

The test suite covers:
- Block-level: hash determinism, tamper detection, genesis reproducibility
- Chain-level: linking, validity, PoW enforcement
- Miner: correct leading zeros, idempotency
- Wallet & transactions: signing, verification, rejection of unsigned txs
- Mempool: submit validation, take, remove, integration with `MineFromMempool`, underfunded rejection, funded acceptance after coinbase
- Node: broadcast propagation, chain acceptance, rejection of shorter/invalid chains
- Wallet: export/import round-trip, save/load from JSON
- Merkle tree: determinism, single tx, empty list, two-tx pair hash, tamper detection

## License

MIT