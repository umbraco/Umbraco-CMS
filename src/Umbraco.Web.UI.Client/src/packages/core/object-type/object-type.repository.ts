import { ObjectTypesService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbObjectTypeRepository extends UmbControllerBase implements UmbApi {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#host = host;
	}

	async #read() {
		return tryExecute(this.#host, ObjectTypesService.getObjectTypes({}));
	}

	async read() {
		const { data, error } = await this.#read();

		return { data, error };
	}
}
