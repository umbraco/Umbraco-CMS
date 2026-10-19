import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbOffsetPaginationRequestModel } from '@umbraco-cms/backoffice/utils';

export interface UmbContentTypeStructureRepository<ItemType> {
	/**
	 * Returns a single page of the content types allowed as children. The server returns the first 100 when
	 * no paging is given, so use `requestAllAllowedChildrenOf` when every allowed child is needed.
	 */
	requestAllowedChildrenOf(
		unique: string | null,
		parentContentUnique: string | null,
		paging?: UmbOffsetPaginationRequestModel,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;

	/**
	 * Returns every content type allowed as a child, paging through the data source until all have been retrieved.
	 */
	requestAllAllowedChildrenOf?(
		unique: string | null,
		parentContentUnique: string | null,
	): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;

	requestAllowedParentsOf?(unique: string): Promise<UmbDataSourceResponse<Array<UmbEntityModel>>>;
}
