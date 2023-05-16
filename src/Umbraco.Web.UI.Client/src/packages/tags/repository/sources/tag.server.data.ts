import { v4 as uuidv4 } from 'uuid';
import { TagResource } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Tag that fetches data from the server
 * @export
 * @class UmbTagServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTagServerDataSource {
	#host: UmbControllerHostElement;

	/**
	 * Creates an instance of UmbTagServerDataSource.
	 * @param {UmbControllerHostElement} host
	 * @memberof UmbTagServerDataSource
	 */
	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Get a list of tags on the server
	 * @return {*}
	 * @memberof UmbTagServerDataSource
	 */
	async getCollection({
		query,
		skip,
		take,
		tagGroup,
		culture,
	}: {
		query: string;
		skip: number;
		take: number;
		tagGroup?: string;
		culture?: string;
	}) {
		return tryExecuteAndNotify(this.#host, TagResource.getTag({ query, skip, take, tagGroup, culture }));
	}
}
