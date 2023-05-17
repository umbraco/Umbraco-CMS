import { DataSourceResponse } from './data-source-response.interface';
import { CreateFolderRequestModel, FolderReponseModel, UpdateFolderReponseModel } from 'src/libs/backend-api';

export interface UmbFolderDataSource {
	createScaffold(parentId: string | null): Promise<DataSourceResponse<FolderReponseModel>>;
	get(unique: string): Promise<DataSourceResponse<FolderReponseModel>>;
	insert(data: CreateFolderRequestModel): Promise<DataSourceResponse<string>>;
	update(unique: string, data: CreateFolderRequestModel): Promise<DataSourceResponse<UpdateFolderReponseModel>>;
	delete(unique: string): Promise<DataSourceResponse>;
}
