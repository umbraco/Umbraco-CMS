import { DocumentModel } from '@umbraco-cms/backend-api';
import type { DataSourceResponse } from '@umbraco-cms/models';
import { RepositoryDetailDataSource } from '@umbraco-cms/repository';

export interface UmbDocumentDataSource extends RepositoryDetailDataSource<DocumentModel> {
	createScaffold(documentTypeKey: string): Promise<DocumentModel>;
	trash(key: string): Promise<DataSourceResponse<DocumentModel>>;
}
