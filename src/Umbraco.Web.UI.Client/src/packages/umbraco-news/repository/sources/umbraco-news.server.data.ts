import type { UmbNewsDataSource } from './index.js';
import { NewsDashboardService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the news items
 * @class UmbNewsServerDataSource
 * @implements {UmbNewsDataSource}
 */
export class UmbNewsServerDataSource implements UmbNewsDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbNewsServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbNewsServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get all news items from the server
	 * @returns {*}
	 * @memberof UmbNewsServerDataSource
	 */
	async getNewsItems() {
		return await tryExecute(this.#host, NewsDashboardService.getNewsDashboard());
	}
}