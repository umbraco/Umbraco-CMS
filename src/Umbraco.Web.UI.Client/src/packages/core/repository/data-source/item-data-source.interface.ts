import type { DataSourceResponse } from 'src/packages/core/repository';

export interface UmbItemDataSource<ItemType> {
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemType>>>;
}
