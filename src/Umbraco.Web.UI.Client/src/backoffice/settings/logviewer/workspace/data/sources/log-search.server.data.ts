import { LogSearchDataSource } from '.';
import { LogViewerResource } from '@umbraco-cms/backend-api';
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
	 * Gets the user's saved searches
	 * @param {string} key
	 * @return {*}
	 * @memberof UmbLogSearchesServerDataSource
	 */
	async getLogViewerSavedSearch() {
		return await tryExecuteAndNotify(this.#host, LogViewerResource.getLogViewerSavedSearch({}));
	}
}
