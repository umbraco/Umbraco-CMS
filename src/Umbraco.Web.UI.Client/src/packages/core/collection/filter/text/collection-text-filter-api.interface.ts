import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbCollectionTextFilterApi extends UmbApi {
	/**
	 * Update the text filter value
	 * @param {string} value The filter text value
	 */
	updateTextFilter(value: string): void;
}
