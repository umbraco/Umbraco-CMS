import type { UmbPropertyEditorDataSourceItemModel } from './types.js';
import type { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';

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
		debugger;
	}
}
