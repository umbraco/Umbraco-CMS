const { http, HttpResponse, delay } = window.MockServiceWorker;
import { umbNewsData } from '../data/umbraco-news.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/news-dashboard'), async () => {
		await delay();
		const response = umbNewsData;
		return HttpResponse.json(response);
	}),
];
