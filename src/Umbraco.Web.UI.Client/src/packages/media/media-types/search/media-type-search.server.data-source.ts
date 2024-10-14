import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaTypeSearchItemModel } from './media-type.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbMediaTypeSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaTypeSearchServerDataSource implements UmbSearchDataSource<UmbMediaTypeSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaTypeSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaTypeSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param args
	 * @returns {*}
	 * @memberof UmbMediaTypeSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MediaTypeService.getItemMediaTypeSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMediaTypeSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/settings/workspace/media-type/edit/' + item.id,
					entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
					unique: item.id,
					name: item.name,
					icon: item.icon || null,
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
