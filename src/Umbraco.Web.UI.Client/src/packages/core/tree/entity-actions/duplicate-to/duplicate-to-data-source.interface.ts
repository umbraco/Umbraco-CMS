import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDuplicateToDataSource {
	/**
	 * Duplicates an item to a destination.
	 * @param {UmbDuplicateToRequestArgs} args - The item to duplicate and its destination.
	 * @returns {Promise<UmbDataSourceResponse<string>>} The unique of the created duplicate when available, or an error.
	 */
	duplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbDataSourceResponse<string>>;
}
