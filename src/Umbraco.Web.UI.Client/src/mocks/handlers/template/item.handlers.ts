const { http, HttpResponse } = window.MockServiceWorker;
import { umbTemplateMockDb } from '../../data/template/template.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const ids = new URL(request.url).searchParams.getAll('id');
		if (!ids) return;
		const items = umbTemplateMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
