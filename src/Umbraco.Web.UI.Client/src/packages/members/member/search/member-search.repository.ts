import { UmbMemberSearchServerDataSource } from './member-search.server.data-source.js';
import type { UmbMemberSearchItemModel } from './types.js';
import type { UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbMemberSearchRepository
	extends UmbControllerBase
	implements UmbSearchRepository<UmbMemberSearchItemModel>, UmbApi
{
	#dataSource: UmbMemberSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbMemberSearchServerDataSource(this);
	}

	search(args: UmbSearchRequestArgs) {
		return this.#dataSource.search(args);
	}
}
