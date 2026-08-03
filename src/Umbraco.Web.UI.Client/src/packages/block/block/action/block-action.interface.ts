import type { UmbBlockActionArgs } from './types.js';
import type { UmbAction } from '@umbraco-cms/backoffice/action';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';

export interface UmbBlockAction<ArgsMetaType> extends UmbAction<UmbBlockActionArgs<ArgsMetaType>> {
	/**
	 * The href location, the action will act as a link.
	 * The `execute` method will not be called.
	 * @returns {Promise<string | undefined>}
	 */
	getHref(): Promise<string | undefined>;

	/**
	 * An optional reactive observable for the href location.
	 * When provided, the default kind element subscribes to it and updates the link reactively,
	 * rather than resolving `getHref()` once at initialisation time.
	 */
	href?: Observable<string | undefined>;

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

	/**
	 * An optional reactive observable for the validation data path.
	 * When provided, the default kind element subscribes to it and updates the validation
	 * state controller reactively, rather than resolving `getValidationDataPath()` once at
	 * initialisation time.
	 */
	validationDataPath?: Observable<string | undefined>;
}
