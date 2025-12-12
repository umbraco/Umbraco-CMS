import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ServerService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbTiptapUmbracoPathConfigServerDataSource {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	getConfig() {
		return tryExecute(this.#host, ServerService.getServerConfiguration(), { disableNotifications: true });
	}
}
