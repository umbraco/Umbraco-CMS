import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

export interface UmbCollectionTextFilterApi extends UmbApi {
	/**
	 * Observable for the current text filter value
	 */
	text: Observable<string | undefined>;

	/**
	 * Update the text filter value
	 * @param {string} value The filter text value
	 */
	setText(value: string): void;
}
