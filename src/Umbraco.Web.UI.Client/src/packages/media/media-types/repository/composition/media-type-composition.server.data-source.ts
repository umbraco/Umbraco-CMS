import type {
	UmbMediaTypeCompositionCompatibleModel,
	UmbMediaTypeCompositionReferenceModel,
	UmbMediaTypeAvailableCompositionRequestModel,
} from '../../types.js';
import { type MediaTypeCompositionRequestModel, MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbContentTypeCompositionDataSource } from '@umbraco-cms/backoffice/content-type';

/**
 * A data source for the Media Type Composition that fetches data from the server
 * @class UmbMediaTypeCompositionServerDataSource
 */
export class UmbMediaTypeCompositionServerDataSource
	implements
		UmbContentTypeCompositionDataSource<
			UmbMediaTypeCompositionReferenceModel,
			UmbMediaTypeCompositionCompatibleModel,
			UmbMediaTypeAvailableCompositionRequestModel
		>
{
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
	 * @param {string} unique
	 * @returns {*}
	 * @memberof UmbMediaTypeCompositionServerDataSource
	 */
	async getReferences(unique: string) {
		const response = await tryExecute(
			this.#host,
			MediaTypeService.getMediaTypeByIdCompositionReferences({ id: unique }),
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
	 * @param {MediaTypeCompositionRequestModel} requestBody
	 * @param args
	 * @returns {*}
	 * @memberof UmbMediaTypeCompositionServerDataSource
	 */
	async availableCompositions(args: UmbMediaTypeAvailableCompositionRequestModel) {
		const requestBody: MediaTypeCompositionRequestModel = {
			id: args.unique,
			currentCompositeIds: args.currentCompositeUniques,
			currentPropertyAliases: args.currentPropertyAliases,
		};

		const response = await tryExecute(this.#host, MediaTypeService.postMediaTypeAvailableCompositions({ requestBody }));
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
