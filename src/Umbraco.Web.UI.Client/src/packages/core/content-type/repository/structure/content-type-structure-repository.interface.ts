import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeStructureRepository<ItemType> {
	requestAllowedChildrenOf(unique: string): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;
}
