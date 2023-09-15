import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { DocumentItemResponseModel, DocumentResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Document items that fetches data from the server
 * @export
 * @class UmbDocumentItemServerDataSource
 * @implements {DocumentItemDataSource}
 */
export class UmbDocumentItemServerDataSource implements UmbItemDataSource<DocumentItemResponseModel> {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbDocumentItemServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbDocumentItemServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches the items for the given ids from the server
	 * @param {Array<string>} ids
	 * @memberof UmbDocumentItemServerDataSource
	 */
	async getItems(ids: Array<string>) {
		if (!ids) throw new Error('Ids are missing');
		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.getDocumentItem({
				id: ids,
			}),
		);
	}
}
