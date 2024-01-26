import type { DataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeStructureRepository<ItemType> {
	requestAllowedAtRoot(): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
	requestAllowedChildrenOf(unique: string): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
}
