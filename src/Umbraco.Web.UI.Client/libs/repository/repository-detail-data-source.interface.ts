import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbDataSource<T> {
	createScaffold(parentKey: string | null): Promise<DataSourceResponse<T>>;
	get(key: string): Promise<DataSourceResponse<T>>;
	insert(data: T): Promise<DataSourceResponse<T>>;
	update(data: T): Promise<DataSourceResponse<T>>;
	delete(key: string): Promise<DataSourceResponse<T>>;
}
