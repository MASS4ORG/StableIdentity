# MASS4 StableIdentity

It provides stable IDs for your data and types. No matter how much you refactor, rename, or reorganize your code, an item's identity stays completely unchanged. Different subsystems in your application can all agree on what a piece of data is, using a single reference.

Lightweight, no dependencies, support for trimming and NativeAOT, zero reflection, CLR type binding, or serialization.

## Contract

- `StableId` wraps a GUID. Its canonical text form is the lowercase hyphenated `D` format; `Parse`/`TryParse` accept only that form, case-insensitively. `StableId.Empty` is never registered or renamed. Identities are never derived from CLR type names, so renaming code does not invalidate saved data.
- `RenameMap` records that a previous identity now resolves to a current one. Chains are followed by `Canonicalize`; an identity has at most one rename entry, and entries that would form a cycle are rejected. Renames are permanent read compatibility: keep them as long as old data may exist.
- `IdentityRegistry<T>` resolves identities to `ITypeIdentity` entries through its own rename map. A renamed identity cannot be registered and a registered identity cannot be renamed. `DisplayName` is for tools and diagnostics only and never takes part in resolution.
- Rename maps and registries are not synchronized. Build them during startup or registration, then read them from any thread.

## Packages

```
dotnet package add MASS4.StableIdentity
```
