import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbNewsServerDataSource } from './umbraco-news.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { NewsDashboardResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbNewsDashboardRepository extends UmbRepositoryBase {
	#host: UmbControllerHost;
	#dataSource: UmbNewsServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#host = host;
		this.#dataSource = new UmbNewsServerDataSource(this.#host);
	}

	async getNewsDashboard(): Promise<UmbDataSourceResponse<NewsDashboardResponseModel>> {
		return this.#dataSource.getNewsItems();
	}
}
