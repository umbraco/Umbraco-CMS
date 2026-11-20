---
name: no-arrange-act-assert-test-comments
description: "Don't add // Arrange / // Act / // Assert section-header comments to new tests in this repo"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 3629769a-7eca-4d89-9fd7-e4c33337b0f8
  modified: 2026-08-18T07:09:34.053Z
---

Don't add `// Arrange`, `// Act`, `// Assert` comments when writing new test methods in this repo, even
though some pre-existing tests use them.

**Why:** User explicitly corrected this while reviewing a new integration test
(`ContentServiceTests.GetAncestors_Guid_Returns_Ancestors_Of_Content`) that copied the style from an
adjacent existing test. This lines up with the repo's general [[feedback_no_section_header_comments]]
policy (no `// --- Section Name ---` banner comments) — Arrange/Act/Assert labels are the same pattern
applied to tests specifically.

**How to apply:** When adding a new test, don't add these labels — let blank lines and code structure
carry the separation instead. Don't retroactively strip them from existing tests that already have them
unless asked; only avoid adding new ones.
