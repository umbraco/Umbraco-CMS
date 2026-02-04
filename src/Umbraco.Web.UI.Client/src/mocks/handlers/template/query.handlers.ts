const { http, HttpResponse } = window.MockServiceWorker;
import { umbTemplateMockDb } from '../../data/template/template.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const queryHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/query/settings`), () => {
		const response = umbTemplateMockDb.query.getQuerySettings();
		return HttpResponse.json(response);
	}),

	http.post(umbracoPath(`${UMB_SLUG}/query/execute`), () => {
		const response = umbTemplateMockDb.query.getQueryResult();
		return HttpResponse.json(response);
	}),
];
