import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbNewsServerDataSource } from './sources/umbraco-news.server.data.js';
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
