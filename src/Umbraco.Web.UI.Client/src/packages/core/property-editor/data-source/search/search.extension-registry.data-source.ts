import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE } from '../entity.js';
import type { UmbSearchDataSource, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
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
	async search(args: UmbSearchRequestArgs) {
		const extensions = umbExtensionsRegistry.getByType('propertyEditorDataSource');

		// Simple filter by name or alias
		const filteredExtensions = extensions.filter(
			(item) =>
				item.name.toLowerCase().includes(args.query.toLowerCase()) ||
				item.alias.toLowerCase().includes(args.query.toLowerCase()),
		);

		const items: UmbPropertyEditorDataSourceItemModel[] = filteredExtensions.map((extension) => ({
			entityType: UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE,
			unique: extension.alias,
			name: extension.name,
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
