import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbItemDataSource<ItemType> {
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemType>>>;
}
