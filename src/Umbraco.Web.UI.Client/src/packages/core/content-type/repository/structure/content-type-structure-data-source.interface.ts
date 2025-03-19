import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export interface UmbContentTypeStructureDataSourceConstructor<ItemType> {
	new (host: UmbControllerHost): UmbContentTypeStructureDataSource<ItemType>;
}

export interface UmbContentTypeStructureDataSource<ItemType> {
	getAllowedChildrenOf(unique: string | null, parentContentUnique: string | null): Promise<UmbDataSourceResponse<UmbPagedModel<ItemType>>>;
}
