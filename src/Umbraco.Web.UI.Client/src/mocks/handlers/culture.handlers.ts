const { http, HttpResponse } = window.MockServiceWorker;

import { umbCulturesData } from '../data/culture.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/culture'), () => {
		const data = umbCulturesData.get();
		return HttpResponse.json(data);
	}),
];
