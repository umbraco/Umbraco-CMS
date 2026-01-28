import type { UmbDocumentCollectionFilterModel, UmbDocumentCollectionItemModel } from '../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { DirectionModel, DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { DocumentCollectionResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionServerDataSource implements UmbCollectionDataSource<UmbDocumentCollectionItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: UmbDocumentCollectionFilterModel) {
		if (!filter.unique) {
			throw new Error('Unique ID is required to fetch a collection.');
		}

		const query = {
			dataTypeId: filter.dataTypeId ?? '',
			orderBy: filter.orderBy ?? 'updateDate',
			orderCulture: filter.orderCulture ?? 'en-US',
			orderDirection: filter.orderDirection === 'asc' ? DirectionModel.ASCENDING : DirectionModel.DESCENDING,
			filter: filter.filter,
			skip: filter.skip || 0,
			take: filter.take || 100,
		};

		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getCollectionDocumentById({ path: { id: filter.unique }, query }),
		);

		if (data) {
			const items = data.items.map((item: DocumentCollectionResponseModel) => {
				const model: UmbDocumentCollectionItemModel = {
					ancestors: item.ancestors.map((ancestor) => {
						return {
							unique: ancestor.id,
							entityType: UMB_DOCUMENT_ENTITY_TYPE,
						};
					}),
					unique: item.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
					creator: item.creator,
					isProtected: item.isProtected,
					isTrashed: item.isTrashed,
					sortOrder: item.sortOrder,
					updater: item.updater,
					flags: item.flags,
					values: item.values.map((item) => {
						return {
							editorAlias: item.editorAlias,
							alias: item.alias,
							culture: item.culture ?? undefined,
							segment: item.segment ?? undefined,
							value: item.value as string,
						};
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
							createDate: new Date(item.createDate),
							updateDate: new Date(item.updateDate),
							flags: item.flags,
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
