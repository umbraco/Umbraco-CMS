import type { UmbSortChildrenOfArgs, UmbSortChildrenOfByFieldArgs } from './types.js';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbSortChildrenOfRepository extends UmbApi {
	/**
	 * Persists an explicit sort order for the children of an entity.
	 * @param {UmbSortChildrenOfArgs} args - the parent and the ordered children
	 * @returns {Promise<UmbRepositoryErrorResponse>} the result of the operation
	 */
	sortChildrenOf(args: UmbSortChildrenOfArgs): Promise<UmbRepositoryErrorResponse>;

	/**
	 * Sorts the children of an entity by a single field on the server.
	 * Optional: only implemented by repositories whose entity supports server-side field sorting.
	 * @param {UmbSortChildrenOfByFieldArgs} args - the parent, field, direction and optional culture
	 * @returns {Promise<UmbRepositoryErrorResponse>} the result of the operation
	 */
	sortChildrenOfByField?(args: UmbSortChildrenOfByFieldArgs): Promise<UmbRepositoryErrorResponse>;
}
