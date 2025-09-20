import type {
	UmbPropertyEditorDataSourceCollectionFilterModel,
	UmbPropertyEditorDataSourceCollectionItemModel,
} from './types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyEditorDataSourceCollectionExtensionRegistryDataSource
	extends UmbControllerBase
	implements UmbCollectionDataSource<UmbPropertyEditorDataSourceCollectionItemModel>
{
	async getCollection(filter: UmbPropertyEditorDataSourceCollectionFilterModel) {
		console.log(filter);
		console.log(umbExtensionsRegistry);
		debugger;
	}
}
