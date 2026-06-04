const { http, HttpResponse, delay } = window.MockServiceWorker;
import { umbMockManager } from '../mock-manager.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/news-dashboard'), async () => {
		await delay();
		const news = umbMockManager.getDataSet().news ?? [];
		const response = { items: news };
		return HttpResponse.json(response);
	}),
];
