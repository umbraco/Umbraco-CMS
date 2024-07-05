import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaItemModel } from './types.js';
import type { MediaItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source for Media items that fetches data from the server
 * @export
 * @class UmbMediaItemServerDataSource
 * @implements {MediaTreeDataSource}
 */
export class UmbMediaItemServerDataSource extends UmbItemServerDataSourceBase<
	MediaItemResponseModel,
	UmbMediaItemModel
> {
	#host: UmbControllerHost;
	/**
	 * Creates an instance of UmbMediaItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMediaItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
		this.#host = host;
	}
	async search({ query, skip, take }: { query: string; skip: number; take: number }) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			MediaService.getItemMediaSearch({ query, skip, take }),
		);
		const mapped = data?.items.map((item) => mapper(item));
		return { data: mapped, error };
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => MediaService.getItemMedia({ id: uniques });

const mapper = (item: MediaItemResponseModel): UmbMediaItemModel => {
	return {
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
};
