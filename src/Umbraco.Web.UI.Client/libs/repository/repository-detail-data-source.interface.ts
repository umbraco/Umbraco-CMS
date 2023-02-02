import type { DataSourceResponse } from '@umbraco-cms/models';

export interface RepositoryDetailDataSource<DetailType> {
	createScaffold(parentKey: string | null): Promise<DataSourceResponse<DetailType>>;
	get(key: string): Promise<DataSourceResponse<DetailType>>;
	insert(data: DetailType): Promise<DataSourceResponse>;
	update(data: DetailType): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}
