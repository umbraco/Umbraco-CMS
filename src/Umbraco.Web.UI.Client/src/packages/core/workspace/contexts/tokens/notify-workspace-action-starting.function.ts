import type { UmbWorkspaceActionExecutionOptions } from './publishable-workspace-context.interface.js';

/**
 * Invokes the `onActionStarting` callback from {@link UmbWorkspaceActionExecutionOptions}
 * when present, otherwise a no-op.
 *
 * Workspace context implementations should call this at the point where the
 * user has committed to the action (e.g. after a confirmation modal has
 * resolved with a positive result) so that callers can surface in-flight UI
 * only while real work is about to begin.
 *
 * Using this helper at the call site keeps the host method free of the
 * optional-chain branches the callback would otherwise contribute to its
 * cyclomatic complexity.
 * @param {UmbWorkspaceActionExecutionOptions} [options] The execution options forwarded by the caller.
 */
export function notifyWorkspaceActionStarting(options?: UmbWorkspaceActionExecutionOptions): void {
	options?.onActionStarting?.();
}
