const { http, HttpResponse } = window.MockServiceWorker;
import { umbStaticFileMockDb } from '../../data/static-file/static-file.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const paths = url.searchParams.getAll('path');
		if (!paths) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		const items = umbStaticFileMockDb.item.getItems(paths);
		return HttpResponse.json(items);
	}),
];
