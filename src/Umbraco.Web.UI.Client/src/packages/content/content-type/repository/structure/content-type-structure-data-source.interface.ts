import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export interface UmbContentTypeStructureDataSourceConstructor<ItemType> {
	new (host: UmbControllerHost): UmbContentTypeStructureDataSource<ItemType>;
}

export interface UmbContentTypeStructureDataSource<ItemType> {
	/**
	 * Returns a single page of the content types allowed as children.
	 * @param {string | null} unique - The content type to get the allowed children of, or `null` for the root.
	 * @param {string | null} parentContentUnique - The content item the children will be created under, if any.
	 * @param {UmbOffsetPaginationRequestModel} paging - The page to return. Implementations must honour this, as a
	 * source that ignores it returns the same page repeatedly when a caller pages through the full set.
	 * @returns {Promise} A promise resolving to `{ data: { items, total } }`, or `{ error }`.
	 */
	getAllowedChildrenOf(
		unique: string | null,
		parentContentUnique: string | null,
		paging?: UmbOffsetPaginationRequestModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;
	getAllowedParentsOf?(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>>;
}
