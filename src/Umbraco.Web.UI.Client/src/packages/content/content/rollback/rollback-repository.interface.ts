import type { UmbContentRollbackVersionDetailModel, UmbContentRollbackVersionItemModel } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export interface UmbContentRollbackRepository extends UmbApi {
	/**
	 * Requests a page of the versions of an entity, newest first.
	 * @param {string} id - The unique identifier of the entity.
	 * @param {string} [culture] - The culture to request versions for. Omit for culture invariant entities.
	 * @param {number} [skip] - The number of versions to skip. Callers pass a multiple of `take`.
	 * @param {number} [take] - The maximum number of versions to return.
	 * @returns {Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }>}
	 * `items` holds only the versions selected by `skip` and `take`, and `total` is the number of all versions of the
	 * entity in the requested culture, which callers use to page through them.
	 */
	requestVersionsByEntityId(
		id: string,
		culture?: string,
		skip?: number,
		take?: number,
	): Promise<{ data?: { items: Array<UmbContentRollbackVersionItemModel>; total: number }; error?: unknown }>;

	requestVersionById(id: string): Promise<{ data?: UmbContentRollbackVersionDetailModel; error?: unknown }>;

	setPreventCleanup(versionId: string, preventCleanup: boolean): Promise<{ error?: unknown }>;

	rollback(versionId: string, culture?: string): Promise<{ error?: unknown }>;
}
