import type { DocumentResponseModel } from '@umbraco-cms/backoffice/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/backoffice/models';
import { RepositoryDetailDataSource } from '@umbraco-cms/backoffice/repository';

export interface UmbDocumentDataSource extends RepositoryDetailDataSource<DocumentResponseModel> {
	createScaffold(documentTypeKey: string): Promise<DataSourceResponse<DocumentResponseModel>>;
	trash(key: string): Promise<DataSourceResponse<DocumentResponseModel>>;
}
