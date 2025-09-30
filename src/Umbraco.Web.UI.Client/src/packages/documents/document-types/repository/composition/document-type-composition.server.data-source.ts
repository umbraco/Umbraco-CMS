import type {
	UmbDocumentTypeCompositionCompatibleModel,
	UmbDocumentTypeCompositionReferenceModel,
	UmbDocumentTypeAvailableCompositionRequestModel,
} from '../../types.js';
import {
	type DocumentTypeCompositionRequestModel,
	DocumentTypeService,
} from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbContentTypeCompositionDataSource } from '@umbraco-cms/backoffice/content-type';

/**
 * A data source for the Document Type Composition that fetches data from the server
 * @class UmbDocumentTypeCompositionServerDataSource
 */
export class UmbDocumentTypeCompositionServerDataSource
	implements
		UmbContentTypeCompositionDataSource<
			UmbDocumentTypeCompositionReferenceModel,
			UmbDocumentTypeCompositionCompatibleModel,
			UmbDocumentTypeAvailableCompositionRequestModel
		>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbDocumentTypeCompositionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}
	/**
	 * Fetches the compatible compositions for a document type from the server
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async getReferences(unique: string) {
		const response = await tryExecute(
			this.#host,
			DocumentTypeService.getDocumentTypeByIdCompositionReferences({ path: { id: unique } }),
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
	 * @param {DocumentTypeCompositionRequestModel} body
	 * @param args
	 * @returns {*}
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async availableCompositions(args: UmbDocumentTypeAvailableCompositionRequestModel) {
		const body: DocumentTypeCompositionRequestModel = {
			id: args.unique,
			isElement: args.isElement,
			currentCompositeIds: args.currentCompositeUniques,
			currentPropertyAliases: args.currentPropertyAliases,
		};

		const response = await tryExecute(this.#host, DocumentTypeService.postDocumentTypeAvailableCompositions({ body }));
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
