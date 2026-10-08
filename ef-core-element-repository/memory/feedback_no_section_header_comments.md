---
name: no-section-header-comments
description: Never add "// --- Section Name ---" banner comments to group members within a class in this codebase — not this project's style
metadata:
  type: feedback
---

Don't add banner/section-header comments like `// --- AsyncContentRepositoryBase abstract overrides ---`, `// --- Private helpers ---`, `// --- Group 20: Recycle bin ---`, etc. to visually divide a class or test file into regions. This applies to production code and test files alike.

**Why:** The user caught this directly on the `v18/feature/ef-core-document-repository` branch — "You keep adding these section comments like `// --- AsyncContentRepositoryBase abstract overrides ---`, we don't want these, it's against our styling." Found and removed 15 instances across `AsyncDocumentRepository.cs`, `AsyncPublishableContentRepositoryBase.cs`, and `AsyncDocumentRepositoryTest.cs` (scoped to the PR's actual diff against its base branch, not the full history). This is a stricter, more specific rule than the root `CLAUDE.md` "Code Comment Policy" (which already says don't restate what code does) — banner comments are the same failure mode applied at the section/region level instead of the line level: member names, method signatures, interface groupings, and blank-line spacing already communicate structure without a label restating it.

**How to apply:** When splitting a class file into logical groups (interface implementations, abstract overrides, private helpers, test groups), use blank lines and file ordering alone — never a `// --- X ---` (or similarly-styled banner) comment line. If a genuine non-obvious rationale needs stating for a group of members, write a real sentence explaining the *why*, not a restated label of *what the group is*. Applies repo-wide, not just to files this memory names.
