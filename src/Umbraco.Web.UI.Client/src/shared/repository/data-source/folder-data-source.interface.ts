import { DataSourceResponse } from './data-source-response.interface.js';
import {
	CreateFolderRequestModel,
	FolderResponseModel,
	UpdateFolderReponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderDataSource {
	createScaffold(parentId: string | null): Promise<DataSourceResponse<FolderResponseModel>>;
	get(unique: string): Promise<DataSourceResponse<FolderResponseModel>>;
	insert(data: CreateFolderRequestModel): Promise<DataSourceResponse<string>>;
	update(unique: string, data: CreateFolderRequestModel): Promise<DataSourceResponse<UpdateFolderReponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
