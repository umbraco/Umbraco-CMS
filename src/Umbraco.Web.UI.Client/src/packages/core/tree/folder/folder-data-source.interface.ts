import type { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import type { UmbDataSourceResponse, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	create(args: UmbCreateFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	read(unique: string): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	update(args: UmbUpdateFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
