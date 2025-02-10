import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import type { UmbMediaSearchItemModel, UmbMediaSearchRequestArgs } from './types.js';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for the Rollback that fetches data from the server
 * @class UmbMediaSearchServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbMediaSearchServerDataSource
	implements UmbSearchDataSource<UmbMediaSearchItemModel, UmbMediaSearchRequestArgs>
{
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMediaSearchServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbMediaSearchServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Get a list of versions for a data
	 * @param {UmbMediaSearchRequestArgs}args - The arguments for the search
	 * @returns {*}
	 * @memberof UmbMediaSearchServerDataSource
	 */
	async search(args: UmbMediaSearchRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MediaService.getItemMediaSearch({
				query: args.query,
				parentId: args.searchFrom?.unique || undefined,
				allowedMediaTypes: args.allowedContentTypes?.map((mediaReference) => mediaReference.unique),
			}),
		);

		if (data) {
			const mappedItems: Array<UmbMediaSearchItemModel> = data.items.map((item) => {
				return {
					entityType: UMB_MEDIA_ENTITY_TYPE,
					hasChildren: item.hasChildren,
					href: '/section/media/workspace/media/edit/' + item.id,
					isTrashed: item.isTrashed,
					unique: item.id,
					mediaType: {
						collection: item.mediaType.collection ? { unique: item.mediaType.collection.id } : null,
						icon: item.mediaType.icon,
						unique: item.mediaType.id,
					},
					name: item.variants[0]?.name, // TODO: get correct variant name
					parent: item.parent ? { unique: item.parent.id } : null,
					variants: item.variants.map((variant) => {
						return {
							culture: variant.culture || null,
							name: variant.name,
						};
					}),
				};
			});

			return { data: { items: mappedItems, total: data.total } };
		}

		return { error };
	}
}
