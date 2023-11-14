import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbItemDataSourceConstructor<ItemType = any> {
	new (host: UmbControllerHost): UmbItemDataSource<ItemType>;
}
export interface UmbItemDataSource<ItemType = any> {
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemType>>>;
}
