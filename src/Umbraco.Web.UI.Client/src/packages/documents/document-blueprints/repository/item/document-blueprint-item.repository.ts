import { UmbDocumentBlueprintItemServerDataSource } from './document-blueprint-item.server.data-source.js';
import { UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT } from './document-blueprint-item.store.context-token.js';
import type { UmbDocumentBlueprintItemModel } from './types.js';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentBlueprintItemRepository extends UmbItemRepositoryBase<UmbDocumentBlueprintItemModel> {
	#dataSource = new UmbDocumentBlueprintItemServerDataSource(this);

	constructor(host: UmbControllerHost) {
		super(host, UmbDocumentBlueprintItemServerDataSource, UMB_DOCUMENT_BLUEPRINT_ITEM_STORE_CONTEXT);
	}

	async requestItemsByDocumentType(unique: string) {
		return this.#dataSource.getItemsByDocumentType(unique);
	}
}

export { UmbDocumentBlueprintItemRepository as api };
