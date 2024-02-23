import type { UmbMediaCollectionFilterModel, UmbMediaCollectionItemModel } from '../types.js';
import { DirectionModel, MediaResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { MediaCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbMediaCollectionServerDataSource implements UmbCollectionDataSource<UmbMediaCollectionItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(query: UmbMediaCollectionFilterModel) {
		// if (!query.dataTypeId) {
		// 	throw new Error('Data type ID is required to fetch a collection.');
		// }

		const params = {
			id: query.unique ?? '',
			dataTypeId: query.dataTypeId,
			orderBy: query.orderBy ?? 'updateDate',
			orderDirection: query.orderDirection === 'asc' ? DirectionModel.ASCENDING : DirectionModel.DESCENDING,
			filter: query.filter,
			skip: query.skip ?? 0,
			take: query.take ?? 100,
		};

		const { data, error } = await tryExecuteAndNotify(this.#host, MediaResource.getCollectionMedia(params));

		if (data) {
			const items = data.items.map((item: MediaCollectionResponseModel) => {
				// TODO: [LK] Temp solution, review how to get the name from the corresponding variant.
				const variant = item.variants[0];

				const model: UmbMediaCollectionItemModel = {
					unique: item.id,
					createDate: new Date(variant.createDate),
					creator: item.creator,
					icon: item.mediaType.icon,
					name: variant.name,
					updateDate: new Date(variant.updateDate),
					values: item.values.map((item) => {
						return { alias: item.alias, value: item.value };
					}),
				};
				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
