import {
	CreateFolderRequestModel,
	FolderModelBaseModel,
	FolderReponseModel,
	PartialViewResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { DataSourceResponse, UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

//! this is of any type in the backend-api
export type PartialViewGetFolderResponse = {path: string, parentPath: string, name: string} ;

export class UmbPartialViewsFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}
	createScaffold(parentId: string | null): Promise<DataSourceResponse<FolderReponseModel>> {
		throw new Error('Method not implemented.');
	}
	get(unique: string): Promise<DataSourceResponse<PartialViewGetFolderResponse>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getPartialViewFolder({ path: unique }));
	}
	insert(requestBody: CreateFolderRequestModel): Promise<DataSourceResponse<string>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.postPartialViewFolder({ requestBody }));
	}
	update(unique: string, data: CreateFolderRequestModel): Promise<DataSourceResponse<FolderModelBaseModel>> {
		throw new Error('Method not implemented.');
	}
	delete(path: string): Promise<DataSourceResponse<unknown>> {
		return tryExecuteAndNotify(this.#host, PartialViewResource.deletePartialViewFolder({ path }));
	}
}
