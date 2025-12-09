const { http, HttpResponse, delay } = window.MockServiceWorker;
import { data as news } from '../data/umbraco-news.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/news-dashboard'), async () => {
		await delay();
		const response = { items: news };
		return HttpResponse.json(response);
	}),
];
