import type { DataSourceResponse, MediaTypeDetails } from '@umbraco-cms/models';

// TODO => Use models when they exist
export interface MediaTypeDetailDataSource {
	createScaffold(parentKey: string): Promise<DataSourceResponse<MediaTypeDetails>>;
	get(key: string): Promise<DataSourceResponse<MediaTypeDetails>>;
	insert(data: any): Promise<DataSourceResponse>;
	update(data: any): Promise<DataSourceResponse>;
	delete(key: string): Promise<DataSourceResponse>;
}
