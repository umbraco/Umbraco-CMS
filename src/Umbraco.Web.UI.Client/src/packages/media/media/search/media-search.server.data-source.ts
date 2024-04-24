import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaSearchItemModel } from './media.search-provider.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @export
 * @class UmbMediaSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaSearchServerDataSource implements UmbSearchDataSource<UmbMediaSearchItemModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaSearchServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @return {*}
	 * @memberof UmbMediaSearchServerDataSource
	 */
	async search(args: UmbSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MediaService.getItemMediaSearch({
				query: args.query,
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMediaSearchItemModel> = data.items.map((item) => {
				return {
					href: '/section/media/workspace/media/edit/' + item.id,
					entityType: UMB_MEDIA_ENTITY_TYPE,
					unique: item.id,
					isTrashed: item.isTrashed,
					mediaType: {
						unique: item.mediaType.id,
						icon: item.mediaType.icon,
						collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
					},
					variants: item.variants.map((variant) => {
						return {
							culture: variant.culture || null,
							name: variant.name,
						};
					}),
					name: item.variants[0]?.name, // TODO: get correct variant name
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
