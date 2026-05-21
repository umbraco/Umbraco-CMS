import type { UmbWorkspaceActionExecutionOptions } from './workspace-action-execution-options.interface.js';

/**
 * Invokes the `onActionStarting` callback from {@link UmbWorkspaceActionExecutionOptions}
 * when present, otherwise a no-op.
 *
 * Workspace context implementations should call this at the point where the
 * user has committed to the action (e.g. after a confirmation modal has
 * resolved with a positive result) so that callers can surface in-flight UI
 * only while real work is about to begin.
 * @param {UmbWorkspaceActionExecutionOptions} [options] The execution options forwarded by the caller.
 */
export function notifyWorkspaceActionStarting(options?: UmbWorkspaceActionExecutionOptions): void {
	options?.onActionStarting?.();
}
