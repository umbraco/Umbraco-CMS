Content index rebuild snapshots
===============================

These files are the expected output of the tests in ContentIndexRebuildOutputTests. Each test seeds the
same multilingual content - nested block lists, block grid and rich text with blocks, mixing culture
variant, invariant and block level variant values - and compares what it produces with one of these
files, as text.

Why they are here
-----------------
Content indexing is the combined work of the property index value factories (recursing through nested
block values), the value set builders (merging fields per culture) and the index populator (loading and
batching documents). A test of any one part can't show that the indexed output is unchanged, so these
tests pin the output of the whole pipeline: every field and value of every indexed document. Any change
to what gets indexed fails a test, so a refactoring can't change the output unnoticed, and an intended
change shows up as a diff of these files that can be reviewed.

The files
---------
  {index}-{nested}-batch{size}.txt  The value sets a full rebuild of an index produces.
                                    {index} is "internal" (all values) or "published" (published
                                    values only).
                                    {nested} is "summary" (block editor values indexed as a single
                                    summary field) or "explicit" (each nested property also indexed in
                                    its own field), set by Indexing:ExplicitlyIndexEachNestedProperty.
                                    {size} is the number of documents indexed at a time.
  load-all.txt                      The property values IContentService.GetPagedDescendants loads for
                                    all documents.
  load-published.txt                The same for published documents, as loaded for the published index.

Database ids, keys, dates and paths depend on insertion order and timing rather than on indexing, so
they are left out. Remaining GUIDs are replaced with <guid>. Documents are ordered by name.

The output for batch sizes 1 and 1000 currently differs, because indexing a block editor value changes
the shared element types used for every value indexed after it. That is a bug, and these files record
the current behaviour.

Updating them
-------------
When a change to the indexed output is intended, run the tests with the environment variable
UMBRACO_TEST_UPDATE_SNAPSHOTS=true. The tests then write their output over these files instead of
comparing against them. Review the resulting diff before committing it: every changed line is a change
to what sites will have in their indexes.
