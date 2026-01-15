import type { UmbDataSourceResponse } from '../data-source-response.interface.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbItemDataSourceConstructor<ItemType> {
	new (host: UmbControllerHost): UmbItemDataSource<ItemType>;
}

export interface UmbItemDataSource<ItemType> {
	getItems(unique: Array<string>): Promise<UmbDataSourceResponse<Array<ItemType>>>;
}
