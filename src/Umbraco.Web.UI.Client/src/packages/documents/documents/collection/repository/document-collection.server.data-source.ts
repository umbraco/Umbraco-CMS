import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
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
				// TODO: remove in v17.0.0
				const variant = item.variants[0];

				const model: UmbDocumentCollectionItemModel = {
					ancestors: item.ancestors.map((ancestor) => {
						return {
							unique: ancestor.id,
							entityType: UMB_DOCUMENT_ENTITY_TYPE,
						};
					}),
					unique: item.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
					contentTypeAlias: item.documentType.alias,
					createDate: new Date(variant.createDate),
					creator: item.creator,
					icon: item.documentType.icon,
					isProtected: item.isProtected,
					isTrashed: item.isTrashed,
					name: variant.name,
					sortOrder: item.sortOrder,
					state: variant.state,
					updateDate: new Date(variant.updateDate),
					updater: item.updater,
					values: item.values.map((item) => {
						return { alias: item.alias, value: item.value as string };
					}),
					documentType: {
						unique: item.documentType.id,
						icon: item.documentType.icon,
						alias: item.documentType.alias,
					},
					variants: item.variants.map((item) => {
						return {
							name: item.name,
							culture: item.culture ?? null,
							state: item.state,
						};
					}),
				};
				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
