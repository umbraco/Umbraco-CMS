import type { UmbEntityActionArgs } from './types.js';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

/**
 * Interface for an entity action.
 * @interface UmbEntityAction
 */
export interface UmbEntityAction<ArgsMetaType> extends UmbAction<UmbEntityActionArgs<ArgsMetaType>> {
	/**
	 * The href location, the action will act as a link.
	 * @returns {Promise<string | undefined>}
	 */
	getHref(): Promise<string | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}
