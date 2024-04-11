import { UmbDocumentSearchServerDataSource } from './rollback.server.data-source.js';
import type { UmbSearchRequestArgs } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbDocumentSearchRepository extends UmbControllerBase implements UmbApi {
	#dataSource: UmbDocumentSearchServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDocumentSearchServerDataSource(this);
	}

	async search(args: UmbSearchRequestArgs) {
		return await this.#dataSource.search(args);
	}
}
