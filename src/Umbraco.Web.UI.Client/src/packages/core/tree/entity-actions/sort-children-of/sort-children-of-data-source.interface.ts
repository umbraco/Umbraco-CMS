import type { UmbSortChildrenOfArgs, UmbSortChildrenOfByFieldArgs } from './types.js';
import type { UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbSortChildrenOfDataSource {
	/**
	 * Persists an explicit sort order for the children of an entity.
	 * @param {UmbSortChildrenOfArgs} args - the parent and the ordered children
	 * @returns {Promise<UmbDataSourceErrorResponse>} the result of the operation
	 */
	sortChildrenOf(args: UmbSortChildrenOfArgs): Promise<UmbDataSourceErrorResponse>;

	/**
	 * Sorts the children of an entity by a single field on the server.
	 * Optional: only implemented by data sources whose entity supports server-side field sorting.
	 * @param {UmbSortChildrenOfByFieldArgs} args - the parent, field, direction and optional culture
	 * @returns {Promise<UmbDataSourceErrorResponse>} the result of the operation
	 */
	sortChildrenOfByField?(args: UmbSortChildrenOfByFieldArgs): Promise<UmbDataSourceErrorResponse>;
}
