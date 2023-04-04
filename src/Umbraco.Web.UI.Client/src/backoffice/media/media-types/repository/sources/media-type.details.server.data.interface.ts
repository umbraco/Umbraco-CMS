import type { MediaTypeDetails } from '@umbraco-cms/backoffice/models';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/repository';

// TODO => Use models when they exist
export interface MediaTypeDetailDataSource {
	createScaffold(parentId: string): Promise<DataSourceResponse<MediaTypeDetails>>;
	get(id: string): Promise<DataSourceResponse<MediaTypeDetails>>;
	insert(data: any): Promise<DataSourceResponse>;
	update(data: any): Promise<DataSourceResponse>;
	delete(id: string): Promise<DataSourceResponse>;
}
