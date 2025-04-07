import { OEmbedService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the OEmbed that fetches data from a given URL.
 * @class UmbOEmbedServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbOEmbedServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbOEmbedServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbOEmbedServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Fetches markup for the given URL.
	 * @param {string} unique
	 * @memberof UmbOEmbedServerDataSource
	 */
	async getOEmbedQuery({ url, maxWidth, maxHeight }: { url?: string; maxWidth?: number; maxHeight?: number }) {
		return tryExecute(this.#host, OEmbedService.getOembedQuery({ url, maxWidth, maxHeight }));
	}
}
