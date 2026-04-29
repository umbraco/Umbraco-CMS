# Variant/Invariant Configuration Matrix

Covers all combinations across 4 dimensions for a document with a block list property containing a block with a text property.

**Legend:** V = Variant (varies by culture) · I = Invariant (shared across cultures)  
**Status:** ✅ Valid · ❌ Impossible · ⚠️ Technically possible but no culture context

| # | Document | Block List Prop | Block Element Type | Text Prop | Status | Behavior |
|---|----------|-----------------|--------------------|-----------|--------|----------|
| 1 | I | I | I | I | ✅ | Fully invariant — one version of everything, shared across all cultures |
| 2 | I | I | I | V | ❌ | Text can't vary if element type is invariant |
| 3 | I | I | V | I | ⚠️ | Variant ET in an invariant doc — no culture context, effectively behaves like #1 |
| 4 | I | I | V | V | ⚠️ | Same as #3 — text is technically variant but unreachable; effectively like #1 |
| 5 | I | V | I | I | ❌ | Block list property can't vary on an invariant document |
| 6 | I | V | I | V | ❌ | Same — plus text can't vary if ET is invariant |
| 7 | I | V | V | I | ❌ | Block list property can't vary on an invariant document |
| 8 | I | V | V | V | ❌ | Same |
| 9 | V | I | I | I | ✅ | Document varies; blocks and text are **shared structure and content** across all cultures |
| 10 | V | I | I | V | ❌ | Text can't vary if element type is invariant |
| 11 | V | I | V | I | ✅ | Document varies; **shared block structure**; text is the same in all cultures (other ET props may vary) |
| 12 | V | I | V | V | ✅ | Document varies; **shared block structure**; text **varies per culture** — same layout, culture-specific text |
| 13 | V | V | I | I | ✅ | Document varies; **each culture has its own block list** (different blocks/order); text is the same across cultures |
| 14 | V | V | I | V | ❌ | Text can't vary if element type is invariant |
| 15 | V | V | V | I | ✅ | Document varies; **each culture has its own block list**; text is invariant within each block |
| 16 | V | V | V | V | ✅ | Fully variant — each culture has its own block list with culture-specific text in each block |

## Valid configurations in plain English

| # | Use case |
|---|----------|
| **1** | No translation at all — one set of content for all cultures |
| **9** | Translated document, but the block list section is entirely shared (no culture-specific blocks or text) |
| **11** | Shared block list structure; block text is identical across cultures but other block properties can differ |
| **12** | Shared block layout across cultures, text content translated per culture — "same template, different copy" |
| **13** | Each culture has a completely different arrangement of blocks, but the text within each block is not translated |
| **15** | Each culture manages its own block list; text in blocks is invariant |
| **16** | Full per-culture control — different blocks, different text, everything can be different |

The most practically useful ones are **#1**, **#12**, and **#16** — fully invariant, shared layout + translated text, and fully variant respectively.
