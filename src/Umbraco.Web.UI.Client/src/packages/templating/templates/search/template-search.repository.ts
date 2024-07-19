import { UmbTemplateSearchServerDataSource } from './template-search.server.data-source.js';
import type { UmbTemplateSearchItemModel } from './template.search-provider.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbTemplateSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbTemplateSearchItemModel>, UmbApi
{
	#dataSource: UmbTemplateSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbTemplateSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
