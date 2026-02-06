import { UMB_EXTENSION_ENTITY_TYPE } from '../../entity.js';
import type { UmbExtensionCollectionFilterModel, UmbExtensionCollectionItemModel } from './types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbExtensionCollectionExtensionRegistryDataSource
	extends UmbControllerBase
	implements UmbCollectionDataSource<UmbExtensionCollectionItemModel>
{
	async getCollection(args: UmbExtensionCollectionFilterModel) {
		const extensions = umbExtensionsRegistry.getAllExtensions();

		const extensionsWithAllowedTypes = extensions.filter((ext) => {
			if (args.extensionTypes && args.extensionTypes.length > 0) {
				return args.extensionTypes.includes(ext.type);
			}
			return true;
		});

		const filtered = extensionsWithAllowedTypes.filter((manifest) =>
			manifest.name.toLowerCase().includes(args.filter?.toLowerCase() ?? ''),
		);

		const extensionsOrderedByName = filtered.sort((a, b) => a.name.localeCompare(b.name));

		const skip = args.skip ?? 0;
		const take = args.take ?? 100;

		const paged = extensionsOrderedByName.slice(skip, skip + take);

		const items: Array<UmbExtensionCollectionItemModel> = paged.map((manifest) => ({
			entityType: UMB_EXTENSION_ENTITY_TYPE,
			unique: manifest.alias,
			name: manifest.name,
			icon: (manifest as any).meta?.icon ?? 'icon-plugin',
			type: manifest.type,
		}));

		return {
			data: {
				items,
				total: filtered.length,
			},
		};
	}
}
