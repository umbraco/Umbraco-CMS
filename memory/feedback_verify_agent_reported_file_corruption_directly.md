---
name: verify-agent-reported-file-corruption-directly
description: "A review subagent reported a single identifier on disk had gotten spliced with stray English-word text mid-token (e.g. AsyncPubliHow shableContentServiceBase) — verified real via hexdump rather than trusting or dismissing the claim, fixed directly, then used a full compiler rebuild (not fragile regex) to confirm nothing else was affected"
metadata: 
  node_type: memory
  type: feedback
  originSessionId: 6b21014d-456d-4e72-9697-bec2de885da6
  modified: 2026-08-10T11:00:22.786Z
---

A background review agent reported that `src/Umbraco.Core/Services/ContentService.cs` line 28 contained `AsyncPubliHow shableContentServiceBase<IContent>` — the literal word "How " spliced into the middle of the identifier `AsyncPublishableContentServiceBase`, breaking the build with a syntax error. The agent said its own first `git diff` at the start of its review had shown the line clean, and the corruption appeared partway through its run without it having edited the file.

**Why this needed direct verification, not trust-or-dismiss**: an agent claiming "the file changed underneath me and I didn't do it" is exactly the kind of surprising claim that could itself be a hallucination, a misread terminal artifact, OR a genuine problem — the only way to know which is to look at the raw bytes myself. `sed -n '28p' file | xxd` confirmed the literal ASCII bytes `48 6f 77 20` ("How ") really were on disk at that exact position. Treating this as unverified and either blindly fixing it OR blindly dismissing it as an agent error would both have been wrong; hex-dumping the actual byte content settled it immediately.

**How to apply:**
1. When any agent (reviewer or otherwise) reports file content that looks impossible or bizarre (a stray word inside an identifier, content that contradicts what you just wrote), verify directly against the live file — `Read`, or `xxd`/hexdump for byte-level certainty — before acting on or dismissing the claim.
2. Once confirmed real, fix the specific corruption directly (a targeted `Edit`), then re-verify the fix compiles.
3. Don't try to hunt for "similar" corruption elsewhere with a clever regex (a first attempt using a pattern like `[a-z][A-Z][a-z]{1,3} [a-z]{3,}[A-Z]` produced dozens of false positives on completely normal C# like `IContentType contentType` — camelCase-variable-after-PascalCase-type is indistinguishable from a splice by shape alone). Instead, force a full non-incremental rebuild (`dotnet build ... --no-incremental`) of every touched project — a broken identifier is a guaranteed compiler error, so the compiler is a far more reliable and much cheaper detector than any text-pattern heuristic.
4. This was an isolated, one-time event with no repeat and no identified cause (possibly a concurrent write from another process/agent touching the same working directory, though never confirmed) — unlike [[feedback_watch_for_stray_file_changes]] (a recurring, meaningful config mutation), there's no signal here to investigate further once the fix is confirmed and the rebuild is clean everywhere.
