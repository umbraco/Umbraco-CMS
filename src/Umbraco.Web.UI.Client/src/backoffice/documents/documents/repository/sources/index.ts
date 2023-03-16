import { DocumentResponseModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';
import { RepositoryDetailDataSource } from '@umbraco-cms/repository';

export interface UmbDocumentDataSource extends RepositoryDetailDataSource<DocumentResponseModel> {
	createScaffold(documentTypeKey: string): Promise<DocumentResponseModel>;
	trash(key: string): Promise<DataSourceResponse<DocumentResponseModel>>;
}
