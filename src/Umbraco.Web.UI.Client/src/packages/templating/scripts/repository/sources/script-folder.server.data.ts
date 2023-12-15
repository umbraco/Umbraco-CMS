import { CreateFolderRequestModel, ScriptResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbFolderDataSource } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbScriptFolderServerDataSource implements UmbFolderDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	createScaffold(): any {
		throw new Error('Method not implemented.');
	}

	read(unique: string) {
		return tryExecuteAndNotify(this.#host, ScriptResource.getScriptFolder({ path: unique }));
	}

	create(requestBody: CreateFolderRequestModel) {
		return tryExecuteAndNotify(this.#host, ScriptResource.postScriptFolder({ requestBody }));
	}

	update(): any {
		throw new Error('Method not implemented.');
	}

	delete(path: string) {
		return tryExecuteAndNotify(this.#host, ScriptResource.deleteScriptFolder({ path }));
	}
}
