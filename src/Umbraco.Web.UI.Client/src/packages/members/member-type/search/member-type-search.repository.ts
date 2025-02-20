import { UmbMemberTypeSearchServerDataSource } from './member-type-search.server.data-source.js';
import type { UmbMemberTypeSearchItemModel } from './member-type.search-provider.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberTypeSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbMemberTypeSearchItemModel>, UmbApi
{
	#dataSource: UmbMemberTypeSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbMemberTypeSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
