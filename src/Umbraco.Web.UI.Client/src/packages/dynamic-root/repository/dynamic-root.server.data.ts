import { DynamicRootResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DynamicRootRequestModel, DynamicRootResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDynamicRootServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async postDynamicRootQuery(args: DynamicRootRequestModel): Promise<DynamicRootResponseModel | undefined> {
		if (!args.context) throw new Error('Dynamic Root context is missing');
		if (!args.query) throw new Error('Dynamic Root query is missing');

		const requestBody: DynamicRootRequestModel = {
			context: args.context,
			query: args.query,
		};

		const { data } = await tryExecuteAndNotify(this.#host, DynamicRootResource.postDynamicRootQuery({ requestBody }));

		return data;
	}
}
