import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { UmbCopyDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeCopyServerDataSource
 */
export class UmbDataTypeCopyServerDataSource implements UmbCopyDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDataTypeCopyServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDataTypeCopyServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Copy an item for the given unique to the target unique
	 * @param {Array<string>} unique
	 * @return {*}
	 * @memberof UmbDataTypeCopyServerDataSource
	 */
	async copy(unique: string, targetUnique: string | null) {
		if (!unique) throw new Error('Unique is missing');
		if (targetUnique === undefined) throw new Error('Target unique is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeByIdCopy({
				id: unique,
				requestBody: {
					targetId: targetUnique,
				},
			}),
		);
	}
}
