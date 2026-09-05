import type {
	UmbMediaTypeCompositionCompatibleModel,
	UmbMediaTypeCompositionReferenceModel,
	UmbMediaTypeAvailableCompositionRequestModel,
} from '../../types.js';
import { type MediaTypeCompositionRequestModel, MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbContentTypeCompositionDataSource } from '@umbraco-cms/backoffice/content-type';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Media Type Composition that fetches data from the server
 * @class UmbMediaTypeCompositionServerDataSource
 */
export class UmbMediaTypeCompositionServerDataSource implements UmbContentTypeCompositionDataSource<
	UmbMediaTypeCompositionReferenceModel,
	UmbMediaTypeCompositionCompatibleModel,
	UmbMediaTypeAvailableCompositionRequestModel
> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeCompositionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeCompositionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}
	/**
	 * Fetches the compatible compositions for a Media type from the server
	 * @param {string} unique - The unique ID of the media type
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbMediaTypeCompositionReferenceModel>>>} The compatible compositions
	 * @memberof UmbMediaTypeCompositionServerDataSource
	 */
	async getReferences(unique: string): Promise<UmbDataSourceResponse<Array<UmbMediaTypeCompositionReferenceModel>>> {
		const response = await tryExecute(
			this.#host,
			MediaTypeService.getMediaTypeByIdCompositionReferences({ path: { id: unique } }),
		);
		const error = response.error;
		const data: Array<UmbMediaTypeCompositionReferenceModel> | undefined = response.data?.map((reference) => {
			return {
				unique: reference.id,
				icon: reference.icon,
				name: reference.name,
			};
		});

		return { data, error };
	}
	/**
	 * Updates the compositions for a media type on the server
	 * @param {UmbMediaTypeAvailableCompositionRequestModel} args - The composition request arguments
	 * @returns {Promise<UmbDataSourceResponse<Array<UmbMediaTypeCompositionCompatibleModel>>>} The available compositions
	 * @memberof UmbMediaTypeCompositionServerDataSource
	 */
	async availableCompositions(
		args: UmbMediaTypeAvailableCompositionRequestModel,
	): Promise<UmbDataSourceResponse<Array<UmbMediaTypeCompositionCompatibleModel>>> {
		const body: MediaTypeCompositionRequestModel = {
			id: args.unique,
			currentCompositeIds: args.currentCompositeUniques,
			currentPropertyAliases: args.currentPropertyAliases,
		};

		const response = await tryExecute(this.#host, MediaTypeService.postMediaTypeAvailableCompositions({ body }));
		const error = response.error;
		const data: Array<UmbMediaTypeCompositionCompatibleModel> | undefined = response.data?.map((composition) => {
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
