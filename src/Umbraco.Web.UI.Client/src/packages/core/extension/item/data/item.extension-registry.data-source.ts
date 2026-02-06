import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import type { UmbExtensionItemModel } from './types.js';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * A data source for Extension items from the extension registry
 * @class UmbExtensionItemExtensionRegistryDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbExtensionItemExtensionRegistryDataSource implements UmbItemDataSource<UmbExtensionItemModel> {
	async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const extensions = umbExtensionsRegistry.getAllExtensions();

		const items: Array<UmbExtensionItemModel> = extensions
			.filter((manifest) => uniques.includes(manifest.alias))
			.map((manifest) => ({
				entityType: UMB_EXTENSION_ENTITY_TYPE,
				unique: manifest.alias,
				name: manifest.name,
				icon: (manifest as any).meta?.icon ?? 'icon-plugin',
				type: manifest.type,
				description: (manifest as any).meta?.description ?? '',
			}));

		return { data: items };
	}
}
