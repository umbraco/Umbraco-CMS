import type { DataSourceResponse } from '@umbraco-cms/models';

export interface RepositoryDetailDataSource<DetailType> {
	createScaffold(parentKey: string | null): Promise<DataSourceResponse<DetailType>>;
	get(key: string): Promise<DataSourceResponse<DetailType>>;
	insert(data: DetailType): Promise<DataSourceResponse<DetailType>>;
	update(data: DetailType): Promise<DataSourceResponse<DetailType>>;
	delete(key: string): Promise<DataSourceResponse<DetailType>>;
}
