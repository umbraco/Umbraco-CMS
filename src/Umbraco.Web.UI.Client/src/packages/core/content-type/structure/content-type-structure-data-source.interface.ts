import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeStructureDataSourceConstructor<ItemType> {
	new (host: UmbControllerHost): UmbContentTypeStructureDataSource<ItemType>;
}

export interface UmbContentTypeStructureDataSource<ItemType> {
	allowedAtRoot(): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
	allowedChildrenOf(unique: string): Promise<DataSourceResponse<UmbPagedModel<ItemType>>>;
}
