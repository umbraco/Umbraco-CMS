import { type DynamicRootRequestModel, DynamicRootResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDynamicRootServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async postDynamicRootQuery(args: DynamicRootRequestModel) {
		if (!args.context) throw new Error('Dynamic Root context is missing');
		if (!args.query) throw new Error('Dynamic Root query is missing');

		const requestBody: DynamicRootRequestModel = {
			context: args.context,
			query: args.query,
		};

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DynamicRootResource.postDynamicRootQuery({ requestBody }),
		);

		if (!error) {
			return data;
		}

		return { error };
	}

	async getQuerySteps() {
		return await tryExecuteAndNotify(this.#host, DynamicRootResource.getDynamicRootSteps());
	}
}
