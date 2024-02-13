import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbDocumentCollectionFilterModel } from '../types.js';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import type { DocumentTreeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCollectionServerDataSource implements UmbCollectionDataSource<UmbDocumentTreeItemModel> {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	async getCollection(filter: UmbDocumentCollectionFilterModel) {
		// TODO: [LK] Replace the Management API call with the correct endpoint once it is available.
		const { data, error } = await tryExecuteAndNotify(this.#host, DocumentResource.getTreeDocumentRoot(filter));

		if (data) {
			const items = data.items.map((item) => this.#mapper(item));

			console.log('UmbDocumentCollectionServerDataSource.getCollection', [data, items]);

			return { data: { items, total: data.total } };
		}

		return { error };
	}

	// TODO: [LK] Temp solution. Copied from "src\packages\documents\documents\tree\document-tree.server.data-source.ts"
	#mapper = (item: DocumentTreeItemResponseModel): UmbDocumentTreeItemModel => {
		return {
			unique: item.id,
			parentUnique: item.parent ? item.parent.id : null,
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
			noAccess: item.noAccess,
			isTrashed: item.isTrashed,
			hasChildren: item.hasChildren,
			isProtected: item.isProtected,
			documentType: {
				unique: item.documentType.id,
				icon: item.documentType.icon,
				hasCollection: item.documentType.hasListView,
			},
			variants: item.variants.map((variant) => {
				return {
					name: variant.name,
					culture: variant.culture || null,
					state: variant.state,
				};
			}),
			name: item.variants[0]?.name, // TODO: this is not correct. We need to get it from the variants. This is a temp solution.
			isFolder: false,
		};
	};
}
