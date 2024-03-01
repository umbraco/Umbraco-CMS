import type { UmbCreateFolderModel, UmbFolderModel, UmbUpdateFolderModel } from './types.js';
import type { UmbDataSourceResponse, UmbDataSourceErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export interface UmbFolderRepository extends UmbApi {
	createScaffold(parentUnique: string | null): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	create(args: UmbCreateFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	request(unique: string): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	update(args: UmbUpdateFolderModel): Promise<UmbDataSourceResponse<UmbFolderModel>>;
	delete(unique: string): Promise<UmbDataSourceErrorResponse>;
}
