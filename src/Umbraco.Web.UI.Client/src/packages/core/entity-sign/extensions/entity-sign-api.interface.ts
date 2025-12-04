import type { MetaEntitySign } from './entity-sign.extension.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbEntitySignApi extends UmbApi {
	/**
	 * Get the label for this sign
	 * @returns {string} The label
	 */
	getLabel?: () => string;

	/**
	 * An observable that provides the label for this sign
	 * @returns { Observable<string>} A label observable
	 */
	label?: Observable<string>;
}

export interface UmbEntitySignApiArgs<MetaType extends MetaEntitySign = MetaEntitySign> {
	meta: MetaType;
}
