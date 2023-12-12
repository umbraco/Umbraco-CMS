import { DataSourceResponse, UmbDataSourceErrorResponse } from '../data-source/index.js';
import { UmbCreateFolderModel, UmbFolderModel, UmbFolderScaffoldModel, UmbUpdateFolderModel } from './types.js';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export interface UmbFolderRepository extends UmbApi {
	createScaffold(parentUnique: string | null): Promise<DataSourceResponse<UmbFolderScaffoldModel>>;
	create(args: UmbCreateFolderModel): Promise<DataSourceResponse<UmbFolderModel>>;
	request(unique: string): Promise<DataSourceResponse<UmbFolderModel>>;
	update(args: UmbUpdateFolderModel): Promise<DataSourceResponse<UmbFolderModel>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
