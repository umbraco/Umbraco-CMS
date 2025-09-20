import { UmbPropertyEditorDataSourceItemExtensionRegistryDataSource } from './item.extension-registry.data-source.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_CONTEXT } from './item.store.context-token.js';
import type { UmbPropertyEditorDataSourceItemModel } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbItemRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbPropertyEditorDataSourceItemRepository extends UmbItemRepositoryBase<UmbPropertyEditorDataSourceItemModel> {
	constructor(host: UmbControllerHost) {
		super(
			host,
			UmbPropertyEditorDataSourceItemExtensionRegistryDataSource,
			UMB_PROPERTY_EDITOR_DATA_SOURCE_ITEM_STORE_CONTEXT,
		);
	}
}

export { UmbPropertyEditorDataSourceItemRepository as api };
