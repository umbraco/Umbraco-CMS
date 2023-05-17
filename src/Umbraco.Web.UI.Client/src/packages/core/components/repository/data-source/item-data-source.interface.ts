import type { DataSourceResponse } from 'src/libs/repository';

export interface UmbItemDataSource<ItemType> {
	getItems(unique: Array<string>): Promise<DataSourceResponse<Array<ItemType>>>;
}
