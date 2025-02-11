import type { UmbWorkspaceAction } from '../types.js';

export interface UmbWorkspaceActionDefaultKind<ArgsMetaType = never> extends UmbWorkspaceAction<ArgsMetaType> {
	/**
	 * The action has additional options.
	 * @returns {undefined | Promise<boolean | undefined>}
	 */
	hasAddionalOptions?(): Promise<boolean | undefined>;
}
