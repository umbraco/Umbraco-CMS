import type { UmbFolderModel } from './types.js';
import type { UmbRepositoryErrorResponse, UmbRepositoryResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
export interface UmbFolderRepository extends UmbApi {
	createScaffold(preset?: Partial<UmbFolderModel>): Promise<UmbRepositoryResponse<UmbFolderModel>>;
	create(data: UmbFolderModel, parentUnique: string | null): Promise<UmbRepositoryResponse<UmbFolderModel>>;
	request(unique: string): Promise<UmbRepositoryResponse<UmbFolderModel>>;
	update(data: UmbFolderModel): Promise<UmbRepositoryResponse<UmbFolderModel>>;
	delete(unique: string): Promise<UmbRepositoryErrorResponse>;
}
