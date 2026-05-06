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

	/**
	 * Optional validation data path for displaying an invalid badge on the action button.
	 * When provided, the default kind element creates a validation state controller
	 * and renders a badge when validation messages exist at the given path.
	 * @returns {Promise<string | undefined>}
	 */
	getValidationDataPath(): Promise<string | undefined>;
}
