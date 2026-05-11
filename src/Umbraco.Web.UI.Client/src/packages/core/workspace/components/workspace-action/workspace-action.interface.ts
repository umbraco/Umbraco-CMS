import type { UmbWorkspaceActionArgs } from './types.js';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

export interface UmbWorkspaceAction<ArgsMetaType = never> extends UmbAction<UmbWorkspaceActionArgs<ArgsMetaType>> {
	isDisabled: Observable<boolean>;

	/**
	 * Emits `true` when the action has started performing real work (e.g. after
	 * a preceding modal/dialog has been confirmed) and `false` otherwise.
	 *
	 * Consumers (such as the workspace action element) use this to show
	 * in-flight UI - e.g. a button spinner - only while work is actually
	 * happening, not while a user is interacting with a modal.
	 *
	 * Optional; when not exposed, consumers fall back to assuming work
	 * begins immediately on `execute()`.
	 */
	isPending?: Observable<boolean>;

	/**
	 * The href location, the action will act as a link.
	 * @returns {Promise<string | undefined>}
	 */
	getHref?(): Promise<string | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}
