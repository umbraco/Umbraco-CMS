import { LogSearchDataSource } from '.';
import { LogViewerResource, SavedLogSearch } from '@umbraco-cms/backend-api';
import { UmbControllerHostInterface } from '@umbraco-cms/controller';
import { tryExecuteAndNotify } from '@umbraco-cms/resources';

/**
 * A data source for the Template detail that fetches data from the server
 * @export
 * @class UmbLogSearchesServerDataSource
 * @implements {TemplateDetailDataSource}
 */
export class UmbLogSearchesServerDataSource implements LogSearchDataSource {
	#host: UmbControllerHostInterface;

	/**
	 * Creates an instance of UmbLogSearchesServerDataSource.
	 * @param {UmbControllerHostInterface} host
	 * @memberof UmbLogSearchesServerDataSource
	 */
	constructor(host: UmbControllerHostInterface) {
		this.#host = host;
	}

	/**
	 * Grabs all the log viewer saved searches from the server
	 *
	 * @param {{ skip?: number; take?: number }} { skip = 0, take = 100 }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getAllSavedSearches({ skip = 0, take = 100 }: { skip?: number; take?: number }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerSavedSearch({ skip, take }));
	}
	/**
	 * Get a log viewer saved search by name from the server
	 *
	 * @param {{ name: string }} { name }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerSavedSearchByName({ name }));
	}

	/**
	 *	Post a new log viewer saved search to the server
	 *
	 * @param {{ requestBody?: SavedLogSearch }} { requestBody }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async postLogViewerSavedSearch({ requestBody }: { requestBody?: SavedLogSearch }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.postLogViewerSavedSearch({ requestBody }));
	}
	/**
	 * Remove a log viewer saved search by name from the server
	 *
	 * @param {{ name: string }} { name }
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async deleteSavedSearchByName({ name }: { name: string }) {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.deleteLogViewerSavedSearchByName({ name }));
	}
}
