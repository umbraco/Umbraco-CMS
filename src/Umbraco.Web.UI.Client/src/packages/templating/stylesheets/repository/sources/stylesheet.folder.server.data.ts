import { CreateFolderRequestModel, StylesheetResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbStylesheetFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	read(unique: string) {
		return tryExecuteAndNotify(this.#host, StylesheetResource.getStylesheetFolder({ path: unique }));
	}

	create(requestBody: CreateFolderRequestModel) {
		return tryExecuteAndNotify(this.#host, StylesheetResource.postStylesheetFolder({ requestBody }));
	}

	delete(path: string) {
		return tryExecuteAndNotify(this.#host, StylesheetResource.deleteStylesheetFolder({ path }));
	}

	update(): any {
		throw new Error('Method not implemented.');
	}

	createScaffold(): any {
		throw new Error('Method not implemented.');
	}
}
