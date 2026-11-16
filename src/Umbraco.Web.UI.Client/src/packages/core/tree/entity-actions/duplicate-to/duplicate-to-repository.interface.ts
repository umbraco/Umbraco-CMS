import type { UmbDuplicateToRequestArgs } from './types.js';
import type { UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbDuplicateToRepository extends UmbApi {
	/**
	 * Requests an item to be duplicated to a destination.
	 * @param {UmbDuplicateToRequestArgs} args - The item to duplicate and its destination.
	 * @returns {Promise<UmbRepositoryResponse<string>>} The unique of the created duplicate when available, or an error.
	 */
	requestDuplicateTo(args: UmbDuplicateToRequestArgs): Promise<UmbRepositoryResponse<string>>;
}
