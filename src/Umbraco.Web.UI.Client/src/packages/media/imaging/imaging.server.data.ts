import type { UmbImagingResizeModel } from './types.js';
import { ImagingService, type MediaUrlInfoResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbMediaUrlModel } from '@umbraco-cms/backoffice/media';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Imaging Service that resizes a media item from the server
 * @class UmbImagingServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbImagingServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbImagingServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbImagingServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches the URL for the given media items as resized images
	 * @param {string} unique
	 * @param uniques
	 * @param imagingModel
	 * @memberof UmbImagingServerDataSource
	 */
	async getItems(uniques: Array<string>, imagingModel?: UmbImagingResizeModel) {
		if (!uniques.length) throw new Error('Uniques are missing');

		const { data, error } = await tryExecute(
			this.#host,
			ImagingService.getImagingResizeUrls({ id: uniques, ...imagingModel }),
		);

		if (data) {
			const items = data.map((item) => this.#mapper(item));
			return { data: items };
		}

		return { error };
	}

	#mapper(item: MediaUrlInfoResponseModel): UmbMediaUrlModel {
		const url = item.urlInfos[0]?.url;
		return {
			unique: item.id,
			url: url,
		};
	}
}
