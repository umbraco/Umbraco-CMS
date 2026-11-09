import type { UmbCultureDataSource } from './index.js';
import { CultureService, type PagedCultureReponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Language that fetches data from the server
 * @class UmbLanguageServerDataSource
 * @implements {UmbCultureDataSource}
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
	 * @param {object} root0 - Pagination options.
	 * @param {number} root0.skip - Number of items to skip.
	 * @param {number} root0.take - Number of items to take.
	 * @returns {Promise<UmbDataSourceResponse<PagedCultureReponseModel>>} The paginated list of cultures.
	 * @memberof UmbLanguageServerDataSource
	 */
	async getCollection({
		skip,
		take,
	}: {
		skip: number;
		take: number;
	}): Promise<UmbDataSourceResponse<PagedCultureReponseModel>> {
		return tryExecute(this.#host, CultureService.getCulture({ query: { skip, take } }));
	}
}
