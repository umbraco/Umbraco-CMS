import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

/**
 * A tree-level action, rendered in the tree's header to operate on the tree as a whole.
 */
export interface UmbTreeAction extends UmbApi {
	/**
	 * The href location, the action will act as a link.
	 * @returns {undefined | Promise<string | undefined>}
	 */
	getHref?: () => Promise<string | undefined>;

	/**
	 * Determine if the UI should indicate that more options will appear when interacting with this.
	 * @returns {undefined | Promise<boolean | undefined>}
	 */
	hasAdditionalOptions?: () => Promise<boolean | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}

/**
 * Base class for {@link UmbTreeAction} implementations.
 */
export abstract class UmbTreeActionBase extends UmbControllerBase implements UmbTreeAction {
	abstract execute(): Promise<void>;
}
