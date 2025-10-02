import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE } from '../../entity.js';
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
	async getCollection(args: UmbPropertyEditorDataSourceCollectionFilterModel) {
		const extensions = umbExtensionsRegistry.getByType('propertyEditorDataSource');

		const filtered = extensions.filter((manifest) =>
			manifest.name.toLowerCase().includes(args.filter?.toLowerCase() ?? ''),
		);

		const extensionsOrderedByLabel = filtered.sort((a, b) => a.name.localeCompare(b.name));

		const skip = args.skip ?? 0;
		const take = args.take ?? 100;

		const paged = extensionsOrderedByLabel.slice(skip, skip + take);

		const items: Array<UmbPropertyEditorDataSourceCollectionItemModel> = paged.map((manifest) => ({
			entityType: UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE,
			unique: manifest.alias,
			name: manifest.meta.label ?? manifest.name,
			icon: manifest.meta.icon ?? 'icon-box',
		}));

		return {
			data: {
				items,
				total: filtered.length,
			},
		};
	}
}
