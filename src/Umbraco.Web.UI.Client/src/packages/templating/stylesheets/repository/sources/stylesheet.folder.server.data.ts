import {
	CreateFolderRequestModel,
	FolderModelBaseModel,
	FolderResponseModel,
	StylesheetResource,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { DataSourceResponse, UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

//! this is of any type in the backend-api
export type StylesheetGetFolderResponse = { path: string; parentPath: string; name: string };

export class UmbStylesheetFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	get(unique: string): Promise<DataSourceResponse<StylesheetGetFolderResponse>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetFolder({ path: unique }));
	}
	insert(requestBody: CreateFolderRequestModel): Promise<DataSourceResponse<string>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.postStylesheetFolder({ requestBody }));
	}
	delete(path: string): Promise<DataSourceResponse<unknown>> {
		return tryExecuteAndNotify(this.#host, StylesheetResource.deleteStylesheetFolder({ path }));
	}
	update(unique: string, data: CreateFolderRequestModel): Promise<DataSourceResponse<FolderModelBaseModel>> {
		throw new Error('Method not implemented.');
	}
	createScaffold(parentId: string | null): Promise<DataSourceResponse<FolderResponseModel>> {
		throw new Error('Method not implemented.');
	}
}
