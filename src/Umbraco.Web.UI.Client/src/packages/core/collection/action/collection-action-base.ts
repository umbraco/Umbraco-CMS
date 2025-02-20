import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionAction extends UmbApi {
	/**
	 * The href location, the action will act as a link.
	 * @returns {undefined | Promise<string | undefined>}
	 */
	getHref?: () => Promise<string | undefined>;

	/**
	 * Determine if the UI should indicate that more options will appear when interacting with this.
	 * @returns {undefined | Promise<boolean | undefined>}
	 */
	hasAddionalOptions?: () => Promise<boolean | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}

export abstract class UmbCollectionActionBase extends UmbControllerBase implements UmbCollectionAction {
	abstract execute(): Promise<void>;
}
