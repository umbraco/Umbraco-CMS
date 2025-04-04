import { TagService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Tag that fetches data from the server
 * @class UmbTagServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbTagServerDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbTagServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbTagServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of tags on the server
	 * @param root0
	 * @param root0.query
	 * @param root0.skip
	 * @param root0.take
	 * @param root0.tagGroup
	 * @param root0.culture
	 * @returns {*}
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
		return tryExecute(this.#host, TagService.getTag({ query, skip, take, tagGroup, culture }));
	}
}
