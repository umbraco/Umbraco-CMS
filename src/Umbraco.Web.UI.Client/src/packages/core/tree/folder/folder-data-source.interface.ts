import {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
} from '../../repository/data-source/data-source-response.interface.js';
import { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	create(args: UmbCreateFolderModel): Promise<DataSourceResponse<UmbFolderModel>>;
	read(unique: string): Promise<DataSourceResponse<UmbFolderModel>>;
	update(args: UmbUpdateFolderModel): Promise<UmbDataSourceErrorResponse>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
