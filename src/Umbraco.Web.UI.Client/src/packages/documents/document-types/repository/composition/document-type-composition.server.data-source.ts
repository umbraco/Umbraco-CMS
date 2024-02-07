import { type DocumentTypeCompositionRequestModel, DocumentTypeResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Document Type Composition that fetches data from the server
 * @export
 * @class UmbDocumentTypeCompositionServerDataSource
 */
export class UmbDocumentTypeCompositionServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeCompositionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}
	/**
	 * Fetches the compatible compositions for a document type from the server
	 * @param {string} unique
	 * @return {*}
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async getReferences(unique: string) {
		return tryExecuteAndNotify(this.#host, DocumentTypeResource.getDocumentTypeByIdCompositionReferences({id:unique}));
	}
	/**
	 * Updates the compositions for a document type on the server
	 * @param {DocumentTypeCompositionRequestModel} requestBody
	 * @return {*}
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async update(data: any) {
		const requestBody: DocumentTypeCompositionRequestModel = {
			id: '',
			isElement: false,
			currentCompositeIds: [],
			currentPropertyAliases:  [],
		}
		return tryExecuteAndNotify(this.#host, DocumentTypeResource.postDocumentTypeAvailableCompositions({ requestBody }));
	}
}
