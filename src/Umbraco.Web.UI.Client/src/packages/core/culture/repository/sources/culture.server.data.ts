import type { UmbCultureDataSource } from './index.js';
import { CultureService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Language that fetches data from the server
 * @class UmbLanguageServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbCultureServerDataSource implements UmbCultureDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbLanguageServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbLanguageServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of cultures on the server
	 * @param root0
	 * @param root0.skip
	 * @param root0.take
	 * @returns {*}
	 * @memberof UmbLanguageServerDataSource
	 */
	async getCollection({ skip, take }: { skip: number; take: number }) {
		return tryExecute(this.#host, CultureService.getCulture({ query: { skip, take } }));
	}
}
