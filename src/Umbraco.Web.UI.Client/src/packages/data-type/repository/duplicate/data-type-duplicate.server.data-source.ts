import { DataTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbDuplicateDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeDuplicateServerDataSource
 */
export class UmbDataTypeDuplicateServerDataSource implements UmbDuplicateDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeDuplicateServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeDuplicateServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Copy an item for the given unique to the target unique
	 * @param {Array<string>} unique
	 * @return {*}
	 * @memberof UmbDataTypeDuplicateServerDataSource
	 */
	async duplicate(unique: string, targetUnique: string | null) {
		if (!unique) throw new Error('Unique is missing');
		if (targetUnique === undefined) throw new Error('Target unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeService.postDataTypeByIdCopy({
				id: unique,
				requestBody: {
					target: targetUnique ? { id: targetUnique } : null,
				},
			}),
		);
	}
}
