import { DynamicRootService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DynamicRootRequestModel, DynamicRootResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * UmbContentPickerDynamicRootServerDataSource
 * @class UmbContentPickerDynamicRootServerDataSource
 */
export class UmbContentPickerDynamicRootServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get dynamic root
	 * @param {DynamicRootRequestModel} args
	 * @returns {*}  {(Promise<DynamicRootResponseModel | undefined>)}
	 * @memberof UmbContentPickerDynamicRootServerDataSource
	 */
	async getRoot(args: DynamicRootRequestModel): Promise<DynamicRootResponseModel | undefined> {
		if (!args.context) throw new Error('Dynamic Root context is missing');
		if (!args.query) throw new Error('Dynamic Root query is missing');

		const requestBody: DynamicRootRequestModel = {
			context: args.context,
			query: args.query,
		};

		const { data } = await tryExecuteAndNotify(this.#host, DynamicRootService.postDynamicRootQuery({ requestBody }));

		return data;
	}
}
