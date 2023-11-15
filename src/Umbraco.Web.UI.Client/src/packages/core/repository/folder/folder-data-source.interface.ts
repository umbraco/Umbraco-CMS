import { DataSourceResponse } from '../data-source/data-source-response.interface.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { FolderResponseModel, UpdateFolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	createScaffold(parentUnique: string | null): Promise<DataSourceResponse<FolderResponseModel>>;
	get(unique: string): Promise<DataSourceResponse<FolderResponseModel>>;
	insert(unique: string, parentUnique: string | null, name: string): Promise<DataSourceResponse<string>>;
	update(unique: string, name: string): Promise<DataSourceResponse<UpdateFolderResponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
