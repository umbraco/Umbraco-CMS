import type { UmbExtensionItemModel } from '../item/types.js';
import { UMB_EXTENSION_ENTITY_TYPE } from '../entity.js';
import type { UmbExtensionSearchRequestArgs } from './types.js';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * A data source for the Extension search, that uses the Extension Registry to find available extensions.
 * @class UmbExtensionSearchExtensionRegistryDataSource
 * @augments {UmbControllerBase}
 * @implements {UmbSearchDataSource<UmbExtensionItemModel>}
 */
export class UmbExtensionSearchExtensionRegistryDataSource
	extends UmbControllerBase
	implements UmbSearchDataSource<UmbExtensionItemModel>
{
	async search(args: UmbExtensionSearchRequestArgs) {
		const extensions = umbExtensionsRegistry.getAllExtensions();

		const extensionsWithAllowedTypes = extensions.filter((ext) => {
			if (args.extensionTypes && args.extensionTypes.length > 0) {
				return args.extensionTypes.includes(ext.type);
			}
			return true;
		});

		const lowerCaseQuery = args.query.toLowerCase();

		// Simple filter by name or alias
		const filteredExtensions = extensionsWithAllowedTypes.filter(
			(item) => item.name.toLowerCase().includes(lowerCaseQuery) || item.alias.toLowerCase().includes(lowerCaseQuery),
		);

		const items: UmbExtensionItemModel[] = filteredExtensions.map((extension) => ({
			entityType: UMB_EXTENSION_ENTITY_TYPE,
			unique: extension.alias,
			name: extension.name,
			icon: (extension as any).meta?.icon ?? 'icon-plugin',
			type: extension.type,
			description: (extension as any).meta?.description ?? '',
		}));

		return {
			data: {
				items: items,
				total: items.length,
			},
		};
	}
}
