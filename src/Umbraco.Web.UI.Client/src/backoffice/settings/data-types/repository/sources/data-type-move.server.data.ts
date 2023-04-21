import { DataTypeResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbMoveDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Data Type items that fetches data from the server
 * @export
 * @class UmbDataTypeMoveServerDataSource
 */
export class UmbDataTypeMoveServerDataSource implements UmbMoveDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDataTypeMoveServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDataTypeMoveServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Move an item for the given id to the target id
	 * @param {Array<string>} id
	 * @return {*}
	 * @memberof UmbDataTypeMoveServerDataSource
	 */
	async move(id: string, targetId: string) {
		if (!id) throw new Error('Id is missing');
		if (!targetId) throw new Error('Target Id is missing');

		return tryExecuteAndNotify(
			this.#host,
			DataTypeResource.postDataTypeByIdMove({
				id,
				requestBody: {
					targetId,
				},
			})
		);
	}
}
