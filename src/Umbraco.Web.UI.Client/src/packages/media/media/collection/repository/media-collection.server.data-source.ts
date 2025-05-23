import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../types.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../../entity.js';
import { DirectionModel, MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { MediaCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionServerDataSource implements UmbCollectionDataSource<UmbMediaCollectionItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: UmbMediaCollectionFilterModel) {
		const query = {
			id: filter.unique ?? '',
			dataTypeId: filter.dataTypeId,
			orderBy: filter.orderBy ?? 'updateDate',
			orderDirection: filter.orderDirection === 'asc' ? DirectionModel.ASCENDING : DirectionModel.DESCENDING,
			filter: filter.filter,
			skip: filter.skip ?? 0,
			take: filter.take ?? 100,
		};

		const { data, error } = await tryExecute(this.#host, MediaService.getCollectionMedia({ query }));

		if (data) {
			const items = data.items.map((item: MediaCollectionResponseModel) => {
				// TODO: [LK] Temp solution, review how to get the name from the corresponding variant.
				const variant = item.variants[0];

				const model: UmbMediaCollectionItemModel = {
					unique: item.id,
					entityType: UMB_MEDIA_ENTITY_TYPE,
					contentTypeAlias: item.mediaType.alias,
					createDate: new Date(variant.createDate),
					creator: item.creator,
					icon: item.mediaType.icon,
					name: variant.name,
					sortOrder: item.sortOrder,
					updateDate: new Date(variant.updateDate),
					updater: item.creator, // TODO: Check if the `updater` is available for media items. [LK]
					values: item.values.map((item) => {
						return { alias: item.alias, value: item.value as string };
					}),
				};
				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
