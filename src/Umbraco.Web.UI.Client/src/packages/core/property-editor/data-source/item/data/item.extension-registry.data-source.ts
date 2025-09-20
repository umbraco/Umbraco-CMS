import type { UmbPropertyEditorDataSourceItemModel } from './types.js';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * A server data source for Property Editor Data Source items
 * @class UmbPropertyEditorDataSourceItemExtensionRegistryDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbPropertyEditorDataSourceItemExtensionRegistryDataSource
	implements UmbItemDataSource<UmbPropertyEditorDataSourceItemModel>
{
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const extensions = umbExtensionsRegistry.getByType('propertyEditorDataSource');

		const items: Array<UmbPropertyEditorDataSourceItemModel> = extensions
			.filter((manifest) => uniques.includes(manifest.alias))
			.map((manifest) => ({
				entityType: manifest.type,
				unique: manifest.alias,
				name: manifest.meta.label ?? manifest.name,
				icon: manifest.meta.icon ?? 'icon-box',
			}));

		return { data: items };
	}
}
