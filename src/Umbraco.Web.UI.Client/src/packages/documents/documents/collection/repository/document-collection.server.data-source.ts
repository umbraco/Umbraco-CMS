import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../types.js';
import { DirectionModel, DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DocumentCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionServerDataSource implements UmbCollectionDataSource<UmbDocumentCollectionItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(query: UmbDocumentCollectionFilterModel) {
		if (!query.unique) {
			throw new Error('Unique ID is required to fetch a collection.');
		}

		const params = {
			id: query.unique,
			dataTypeId: query.dataTypeId ?? '',
			orderBy: query.orderBy ?? 'updateDate',
			orderCulture: query.orderCulture ?? 'en-US',
			orderDirection: query.orderDirection === 'asc' ? DirectionModel.ASCENDING : DirectionModel.DESCENDING,
			filter: query.filter,
			skip: query.skip || 0,
			take: query.take || 100,
		};

		const { data, error } = await tryExecuteAndNotify(this.#host, DocumentService.getCollectionDocumentById(params));

		if (data) {
			const items = data.items.map((item: DocumentCollectionResponseModel) => {
				// TODO: [LK] Temp solution, review how to get the name from the corresponding variant.
				const variant = item.variants[0];

				const model: UmbDocumentCollectionItemModel = {
					unique: item.id,
					entityType: 'document',
					contentTypeAlias: item.documentType.alias,
					createDate: new Date(variant.createDate),
					creator: item.creator,
					icon: item.documentType.icon,
					name: variant.name,
					sortOrder: item.sortOrder,
					state: variant.state,
					updateDate: new Date(variant.updateDate),
					updater: item.updater,
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
