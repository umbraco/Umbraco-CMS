import type { PagedTagResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { TagService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for the Tag that fetches data from the server
 * @class UmbTagServerDataSource
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
	 * @param {object} root0 - The parameters for the tag search.
	 * @param {string} root0.query - The search query.
	 * @param {number} root0.skip - The number of tags to skip.
	 * @param {number} root0.take - The number of tags to take.
	 * @param {string} [root0.tagGroup] - The tag group to filter by.
	 * @param {string} [root0.culture] - The culture to filter by.
	 * @returns {Promise<UmbDataSourceResponse<PagedTagResponseModel>>} The list of tags.
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
	}): Promise<UmbDataSourceResponse<PagedTagResponseModel>> {
		return tryExecute(this.#host, TagService.getTag({ query: { query, skip, take, tagGroup, culture } }));
	}
}
