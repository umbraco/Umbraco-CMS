import type {
	UmbDocumentTypeCompositionCompatibleModel,
	UmbDocumentTypeCompositionReferenceModel,
	UmbDocumentTypeCompositionRequestModel,
} from '../../types.js';
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
		const response = await tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.getDocumentTypeByIdCompositionReferences({ id: unique }),
		);
		const error = response.error;
		const data: Array<UmbDocumentTypeCompositionReferenceModel> | undefined = response.data?.map((reference) => {
			return {
				unique: reference.id,
				icon: reference.icon,
				name: reference.name,
			};
		});

		return { data, error };
	}
	/**
	 * Updates the compositions for a document type on the server
	 * @param {DocumentTypeCompositionRequestModel} requestBody
	 * @return {*}
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async availableCompositions(args: UmbDocumentTypeCompositionRequestModel) {
		const requestBody: DocumentTypeCompositionRequestModel = {
			id: args.unique,
			isElement: args.isElement,
			currentCompositeIds: args.currentCompositeUniques,
			currentPropertyAliases: args.currentPropertyAliases,
		};

		const response = await tryExecuteAndNotify(
			this.#host,
			DocumentTypeResource.postDocumentTypeAvailableCompositions({ requestBody }),
		);
		const error = response.error;
		const data: Array<UmbDocumentTypeCompositionCompatibleModel> | undefined = response.data?.map((composition) => {
			return {
				unique: composition.id,
				name: composition.name,
				icon: composition.icon,
				folderPath: composition.folderPath,
				isCompatible: composition.isCompatible,
			};
		});

		return { data, error };
	}
}
