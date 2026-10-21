# Extreme Conditions

Two Workspace Contexts for the Document Workspace, each with a condition that takes ten seconds to answer. They are meant for checking how a workspace behaves while the conditions of its Workspace Contexts are slow to resolve.

## Extensions included

- **Slow And Unique** - `Umb.Condition.Delay` (10 seconds) and `Umb.Condition.Entity.Unique`, matching one specific document. For any other document the unique condition answers `false` straight away, so the context is ruled out without waiting for the delay. For the matching document the context is created once the delay has passed.
- **Slow And Negative** - `Umb.Condition.Delay` (10 seconds) and `Umb.Condition.WorkspaceEntityIsNew`. The latter answers `false` for every document that is already saved, so the context is ruled out without waiting for the delay. For a new document the context is created once the delay has passed.

## Trying it

Run `npm run example:mock`, pick this example and open documents in the content section. The unique to match is the `SLOW_AND_UNIQUE_DOCUMENT` constant in `index.ts`.

Opening a document that is ruled out by a `false` condition should load right away, and not wait for the workspace's settle timeout. The browser console logs a message when a context is created.
