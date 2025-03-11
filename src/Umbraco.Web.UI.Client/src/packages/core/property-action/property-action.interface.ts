import type { UmbPropertyActionArgs } from './types.js';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

export interface UmbPropertyAction<ArgsMetaType = never> extends UmbAction<UmbPropertyActionArgs<ArgsMetaType>> {
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
