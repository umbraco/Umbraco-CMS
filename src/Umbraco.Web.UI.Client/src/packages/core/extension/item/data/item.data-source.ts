import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import type { UmbExtensionItemModel } from './types.js';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';

export class UmbExtensionItemDataSource implements UmbItemDataSource<UmbExtensionItemModel> {
	async getItems(uniques: Array<string>) {
		const extensions = umbExtensionsRegistry.getAllExtensions();

		const data: Array<UmbExtensionItemModel> = extensions
			.filter((manifest) => uniques.includes(manifest.alias))
			.map((manifest) => ({
				entityType: UMB_EXTENSION_ENTITY_TYPE,
				unique: manifest.alias,
				name: manifest.name,
				description: manifest.alias,
				icon: 'icon-plugin',
				manifest: {
					type: manifest.type,
					alias: manifest.alias,
					name: manifest.name,
					weight: manifest.weight,
					kind: manifest.kind,
				},
			}));

		return { data };
	}
}
