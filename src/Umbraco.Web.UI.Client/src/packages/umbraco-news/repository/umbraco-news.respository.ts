import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { NewsDashboardService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbNewsDashboardRepository extends UmbRepositoryBase {
	async getNewsDashboard() {
		const { data } = await tryExecute(this, NewsDashboardService.getNewsDashboard());
		return data;
	}
}
