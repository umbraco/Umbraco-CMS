# Content index rebuild snapshots

These files are the expected output of the tests in `ContentIndexRebuildOutputTests`. Each test seeds the same multilingual content and compares what it produces with one of these files, as text. The content includes nested block lists, block grid and rich text with blocks, and mixes culture variant, invariant and block level variant values.

## Why they are here

Content indexing is the combined work of:

- the property index value factories, which recurse through nested block values;
- the value set builders, which merge fields per culture;
- the index populator, which loads and batches documents.

A test of any one part can't show that the indexed output is unchanged. These tests pin the output of the whole pipeline: every field and value of every indexed document.

Any change to what gets indexed fails a test. A refactoring can't change the output unnoticed, and an intended change shows up as a diff of these files that can be reviewed.

## The files

| File | Contents |
|---|---|
| `{index}-{nested}-batch{size}.txt` | The value sets a full rebuild of an index produces. |
| `load-all.txt` | The property values `IContentService.GetPagedDescendants` loads for all documents. |
| `load-published.txt` | The same for published documents, as loaded for the published index. |

In the value set file names:

- `{index}` is `internal` (all values) or `published` (published values only).
- `{nested}` is `summary` (block editor values indexed as a single summary field) or `explicit` (each nested property also indexed in its own field), set by `Indexing:ExplicitlyIndexEachNestedProperty`.
- `{size}` is the number of documents indexed at a time.

Database ids, keys, dates and paths depend on insertion order and timing rather than on indexing, so they are left out. Remaining GUIDs are replaced with `<guid>`, and documents are ordered by name.

The output for batch sizes 1 and 1000 currently differs. Indexing a block editor value changes the shared element types used for every value indexed after it. That is a bug, and these files record the current behaviour.

## Updating them

When a change to the indexed output is intended, run the tests with the environment variable `UMBRACO_TEST_UPDATE_SNAPSHOTS=true`. The tests then write their output over these files instead of comparing against them.

Review the resulting diff before committing it: every changed line is a change to what sites will have in their indexes.
