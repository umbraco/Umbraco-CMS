import type { UmbPropertyEditorDataSourceItemModel } from '../item/types.js';
import { UmbPropertyEditorDataSourceSearchExtensionRegistryDataSource } from './search.extension-registry.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbPropertyEditorDataSourceSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbPropertyEditorDataSourceItemModel>, UmbApi
{
	#dataSource: UmbPropertyEditorDataSourceSearchExtensionRegistryDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbPropertyEditorDataSourceSearchExtensionRegistryDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
