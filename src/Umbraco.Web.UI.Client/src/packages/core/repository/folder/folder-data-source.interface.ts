import { DataSourceResponse } from '../data-source/data-source-response.interface.js';
import { UmbCreateFolderModel, UmbUpdateFolderModel } from './types.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { FolderResponseModel, UpdateFolderResponseModel } from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	create(args: UmbCreateFolderModel): Promise<DataSourceResponse<string>>;
	read(unique: string): Promise<DataSourceResponse<FolderResponseModel>>;
	update(args: UmbUpdateFolderModel): Promise<DataSourceResponse<UpdateFolderResponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
