import type { UmbBlockActionArgs } from './types.js';
import type { UmbAction } from '@umbraco-cms/backoffice/action';

export interface UmbBlockAction<ArgsMetaType> extends UmbAction<UmbBlockActionArgs<ArgsMetaType>> {
	/**
	 * The href location, the action will act as a link.
	 * The `execute` method will not be called.
	 * @returns {Promise<string | undefined>}
	 */
	getHref(): Promise<string | undefined>;

	/**
	 * The `execute` method, the action will act as a button.
	 * @returns {Promise<void>}
	 */
	execute(): Promise<void>;
}
