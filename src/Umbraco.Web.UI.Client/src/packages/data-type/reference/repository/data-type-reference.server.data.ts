import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * @class UmbDataTypeReferenceServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbDataTypeReferenceServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeReferenceServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDataTypeReferenceServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} id
	 * @returns {*}
	 * @memberof UmbDataTypeReferenceServerDataSource
	 */
	async getReferencedBy(id: string) {
		return await tryExecuteAndNotify(this.#host, DataTypeService.getDataTypeByIdReferences({ id }));
	}
}
