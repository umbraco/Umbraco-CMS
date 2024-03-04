import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ServertimeOffset } from '@umbraco-cms/backoffice/models';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export class UmbConfigRepository {
	#host;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getServertimeOffset() {
		const resource = fetch(umbracoPath('/config/servertimeoffset')).then((res) => res.json());
		const { data } = await tryExecuteAndNotify<ServertimeOffset>(this.#host, resource);
		return data;
	}
}
