import { UmbNewsServerDataSource } from './umbraco-news.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { NewsDashboardResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export class UmbNewsDashboardRepository extends UmbRepositoryBase {
	#dataSource = new UmbNewsServerDataSource(this);

	async getNewsDashboard(): Promise<UmbDataSourceResponse<NewsDashboardResponseModel>> {
		return this.#dataSource.getNewsItems();
	}
}
