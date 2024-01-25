import { ObjectTypesResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbObjectTypeRepository extends UmbBaseController implements UmbApi {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#host = host;
	}

	async #read() {
		return tryExecuteAndNotify(this.#host, ObjectTypesResource.getObjectTypes({}));
	}

	async read() {
		const { data, error } = await this.#read();

		return { data, error };
	}
}
