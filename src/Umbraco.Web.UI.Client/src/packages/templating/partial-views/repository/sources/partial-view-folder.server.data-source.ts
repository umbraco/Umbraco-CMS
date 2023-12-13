import { CreateFolderRequestModel, PartialViewResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbPartialViewFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	createScaffold(): any {
		throw new Error('Method not implemented.');
	}

	read(unique: string) {
		return tryExecuteAndNotify(this.#host, PartialViewResource.getPartialViewFolder({ path: unique }));
	}

	create(requestBody: CreateFolderRequestModel) {
		return tryExecuteAndNotify(this.#host, PartialViewResource.postPartialViewFolder({ requestBody }));
	}

	update(): any {
		throw new Error('Method not implemented.');
	}

	delete(path: string) {
		return tryExecuteAndNotify(this.#host, PartialViewResource.deletePartialViewFolder({ path }));
	}
}
