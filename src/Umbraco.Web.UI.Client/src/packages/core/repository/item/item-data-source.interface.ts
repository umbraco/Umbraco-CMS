import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbItemDataSourceConstructor<ItemType> {
	new (host: UmbControllerHost): UmbItemDataSource<ItemType>;
}

export interface UmbItemDataSource<ItemType> {
	getItems(unique: Array<string>): Promise<UmbDataSourceResponse<Array<ItemType>>>;
}
