import { DataSourceResponse } from './data-source-response.interface.js';
import {
	CreateFolderRequestModel,
	FolderResponseModel,
	UpdateFolderResponseModel,
} from '@umbraco-cms/backoffice/backend-api';

export interface UmbFolderDataSource {
	createScaffold(parentId: string | null): Promise<DataSourceResponse<FolderResponseModel>>;
	create(data: CreateFolderRequestModel): Promise<DataSourceResponse<string>>;
	read(unique: string): Promise<DataSourceResponse<FolderResponseModel>>;
	update(unique: string, data: CreateFolderRequestModel): Promise<DataSourceResponse<UpdateFolderResponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
