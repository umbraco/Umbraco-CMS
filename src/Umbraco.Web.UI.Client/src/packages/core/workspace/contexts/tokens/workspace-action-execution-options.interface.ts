/**
 * Options for workspace context methods that may open a modal before doing work.
 */
export interface UmbWorkspaceActionExecutionOptions {
	/**
	 * Invoked when the action has passed any preceding modal/dialog and real
	 * work is about to begin. Allows callers (typically workspace actions) to
	 * surface in-flight UI only while work is actually happening.
	 *
	 * When the user cancels a preceding modal, this callback is not invoked.
	 */
	onActionStarting?: () => void;
}
