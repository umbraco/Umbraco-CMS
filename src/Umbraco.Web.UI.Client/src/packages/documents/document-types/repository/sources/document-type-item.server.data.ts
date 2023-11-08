import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { DocumentTypeItemResponseModel, DocumentTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Document Type items that fetches data from the server
 * @export
 * @class UmbDocumentTypeItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbDocumentTypeItemServerDataSource implements UmbItemDataSource<DocumentTypeItemResponseModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @return {*}
	 * @memberof UmbDocumentTypeItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeItem({
				id: ids,
			}),
		);
	}
}
