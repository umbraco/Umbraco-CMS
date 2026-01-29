const { http, HttpResponse, delay } = window.MockServiceWorker;
import { dataSet } from '../data/sets/index.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const news = dataSet.news ?? [];

export const handlers = [
	http.get(umbracoPath('/news-dashboard'), async () => {
		await delay();
		const response = { items: news };
		return HttpResponse.json(response);
	}),
];
