import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE } from '../entity.js';
import type { UmbPropertyEditorDataSourceSearchRequestArgs } from './types.js';
import type { UmbSearchDataSource } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

/**
 * A data source for the Property Editor Data Source search, that uses the Extension Registry to find available data sources.
 * @class UmbPropertyEditorDataSourceSearchExtensionRegistryDataSource
 * @augments {UmbControllerBase}
 * @implements {UmbSearchDataSource<UmbPropertyEditorDataSourceItemModel>}
 */
export class UmbPropertyEditorDataSourceSearchExtensionRegistryDataSource
	extends UmbControllerBase
	implements UmbSearchDataSource<UmbPropertyEditorDataSourceItemModel>
{
	async search(args: UmbPropertyEditorDataSourceSearchRequestArgs) {
		const extensions = umbExtensionsRegistry.getByType('propertyEditorDataSource');

		const extensionsWithAllowedDataSourceTypes = extensions.filter((ext) => {
			if (args.dataSourceTypes && args.dataSourceTypes.length > 0) {
				return args.dataSourceTypes.includes(ext.dataSourceType);
			}
			return true;
		});

		const lowerCaseQuery = args.query.toLowerCase();

		// Simple filter by name or alias
		const filteredExtensions = extensionsWithAllowedDataSourceTypes.filter(
			(item) =>
				item.meta.label.toLowerCase().includes(lowerCaseQuery) ||
				item.name.toLowerCase().includes(lowerCaseQuery) ||
				item.alias.toLowerCase().includes(lowerCaseQuery),
		);

		const items: UmbPropertyEditorDataSourceItemModel[] = filteredExtensions.map((extension) => ({
			entityType: UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE,
			unique: extension.alias,
			name: extension.meta.label,
			icon: extension.meta?.icon,
		}));

		return {
			data: {
				items: items,
				total: items.length,
			},
		};
	}
}
