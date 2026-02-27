---
name: pr-cleanup
description: Clean up after PR testing. Stops the running Umbraco instance and removes the git worktree created by /pr-test. Use this skill when the user says "clean up", "stop the instance", "remove worktree", "I'm done testing", or "shut it down" in the context of PR testing. Also triggers when user says "/pr-cleanup" with a PR number.
---

# PR Cleanup

You clean up after a PR test session by stopping the running Umbraco instance and removing the git worktree.

## Arguments

- A PR number (e.g., `21887`): clean up that specific PR's instance and worktree
- No arguments: list all PR worktrees and offer to clean them all

## Step 1: Find and Stop the Umbraco Process

Calculate the port: `PORT = 10000 + ({PR_NUMBER} % 10000)`

Note: If `/pr-test` used a non-default port due to a conflict, the user may need to tell you which port was used. Check both the default and common alternatives (20000+, 30000+ base).

Find the process listening on that port:

```bash
lsof -ti:${PORT} 2>/dev/null
```

If a process is found, kill it:

```bash
kill $(lsof -ti:${PORT}) 2>/dev/null
```

Verify it's stopped:

```bash
lsof -ti:${PORT} 2>/dev/null && echo "Process still running" || echo "Process stopped"
```

If the process doesn't stop with a regular kill, use `kill -9` as a last resort.

## Step 2: Remove the Git Worktree

```bash
cd /Users/philw/Projects/Umbraco-CMS
git worktree remove .claude/worktrees/pr-{PR_NUMBER} --force
```

If the worktree removal fails (e.g., uncommitted changes), ask the user if they want to force remove it.

## Step 3: Clean Up the Branch Reference

```bash
git branch -D pr-{PR_NUMBER} 2>/dev/null || true
```

## Step 4: Confirm Cleanup

Report to the user:

```
Cleanup complete for PR #{PR_NUMBER}:
- Stopped Umbraco process on port {PORT}
- Removed worktree at .claude/worktrees/pr-{PR_NUMBER}
- Cleaned up branch ref pr-{PR_NUMBER}
```

If any step failed, report what failed and suggest manual remediation.

## Bulk Cleanup

If no PR number is provided, list all PR worktrees and offer to clean them all:

```bash
ls /Users/philw/Projects/Umbraco-CMS/.claude/worktrees/ 2>/dev/null | grep "^pr-"
```

For each one, show whether it has a running process and ask the user which ones to clean up.
