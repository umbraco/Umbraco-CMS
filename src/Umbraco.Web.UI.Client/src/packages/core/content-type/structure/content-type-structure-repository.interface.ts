import type { DataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeStructureRepository<ItemType> {
	requestAllowedChildrenOf(unique: string): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
}
