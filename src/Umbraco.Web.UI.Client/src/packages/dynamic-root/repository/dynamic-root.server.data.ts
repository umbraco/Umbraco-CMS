import { DynamicRootResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDynamicRootServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	// TODO: Implement `postDynamicRootQuery` [LK]

	async getQuerySteps() {
		return await tryExecuteAndNotify(this.#host, DynamicRootResource.getDynamicRootSteps());
	}
}
