import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { NewsDashboardService, type NewsDashboardResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { umbNewsData } from 'src/mocks/data/umbraco-news.data';

export class UmbNewsDashboardRepository extends UmbRepositoryBase {
	async getNewsDashboard() {
		const res = await tryExecute(this, NewsDashboardService.getNewsDashboard());
		console.log('res in repo', res);
		const data = res.data as NewsDashboardResponseModel | undefined;
		if (!data || !data.items?.length) return umbNewsData;

		return data ?? { items: [] };
	}
}
