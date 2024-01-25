import type {
	DataSourceResponse,
	UmbDataSourceErrorResponse,
} from '../../repository/data-source/data-source-response.interface.js';
import type { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbFolderDataSourceConstructor {
	new (host: UmbControllerHost): UmbFolderDataSource;
}

export interface UmbFolderDataSource {
	create(args: UmbCreateFolderModel): Promise<DataSourceResponse<UmbFolderModel>>;
	read(unique: string): Promise<DataSourceResponse<UmbFolderModel>>;
	update(args: UmbUpdateFolderModel): Promise<DataSourceResponse<UmbFolderModel>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
