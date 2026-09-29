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
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Document Type Composition that fetches data from the server
 * @class UmbDocumentTypeCompositionServerDataSource
 */
export class UmbDocumentTypeCompositionServerDataSource implements UmbContentTypeCompositionDataSource<
	UmbDocumentTypeCompositionReferenceModel,
	UmbDocumentTypeCompositionCompatibleModel,
	UmbDocumentTypeAvailableCompositionRequestModel
> {
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
	 * @param {string} unique - The unique identifier of the document type.
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbDocumentTypeCompositionReferenceModel>>>} The compositions referencing the document type.
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async getReferences(unique: string): Promise<UmbDataSourceResponse<Array<UmbDocumentTypeCompositionReferenceModel>>> {
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
	 * @param {UmbDocumentTypeAvailableCompositionRequestModel} args - The arguments to determine the available compositions.
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbDocumentTypeCompositionCompatibleModel>>>} The compatible compositions for the document type.
	 * @memberof UmbDocumentTypeCompositionServerDataSource
	 */
	async availableCompositions(
		args: UmbDocumentTypeAvailableCompositionRequestModel,
	): Promise<UmbDataSourceResponse<Array<UmbDocumentTypeCompositionCompatibleModel>>> {
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
