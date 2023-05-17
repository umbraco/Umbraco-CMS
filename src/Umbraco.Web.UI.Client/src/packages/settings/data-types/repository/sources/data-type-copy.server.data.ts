import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbCopyDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeCopyServerDataSource
 */
export class UmbDataTypeCopyServerDataSource implements UmbCopyDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeCopyServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeCopyServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Copy an item for the given id to the target id
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbDataTypeCopyServerDataSource
	 */
	async copy(id: string, targetId: string | null) {
		if (!id) throw new Error('Id is missing');
		if (targetId === undefined) throw new Error('Target Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeByIdCopy({
				id,
				requestBody: {
					targetId,
				},
			})
		);
	}
}
