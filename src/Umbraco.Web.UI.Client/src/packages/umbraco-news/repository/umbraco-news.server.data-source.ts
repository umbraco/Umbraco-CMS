import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { NewsDashboardService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { NewsDashboardResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';

export interface UmbNewsDataSource {
	getNewsItems(): Promise<UmbDataSourceResponse<NewsDashboardResponseModel>>;
}

/**
 * A data source for the news items
 * @class UmbNewsServerDataSource
 * @implements {UmbNewsDataSource}
 */
export class UmbNewsServerDataSource extends UmbControllerBase implements UmbNewsDataSource {
	/**
	 * Get all news items from the server
	 * @returns {*}
	 * @memberof UmbNewsServerDataSource
	 */
	async getNewsItems() {
		return await tryExecute(this._host, NewsDashboardService.getNewsDashboard());
	}
}
