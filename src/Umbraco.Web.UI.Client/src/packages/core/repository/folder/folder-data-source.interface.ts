import { DataSourceResponse } from '../data-source/data-source-response.interface.js';
import { UmbCreateFolderModel, UmbUpdateFolderModel } from './types.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { FolderResponseModel, UpdateFolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	get(unique: string): Promise<DataSourceResponse<FolderResponseModel>>;
	insert(args: UmbCreateFolderModel): Promise<DataSourceResponse<string>>;
	update(args: UmbUpdateFolderModel): Promise<DataSourceResponse<UpdateFolderResponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
