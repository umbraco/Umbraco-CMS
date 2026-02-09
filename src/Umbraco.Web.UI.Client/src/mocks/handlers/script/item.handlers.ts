const { http, HttpResponse } = window.MockServiceWorker;
import { umbScriptMockDb } from '../../data/script/script.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const paths = new URL(request.url).searchParams.getAll('path');
		if (!paths) return new HttpResponse(null, { status: 400 });
		const decodedPaths = paths.map((path) => decodeURI(path));
		const items = umbScriptMockDb.item.getItems(decodedPaths);
		return HttpResponse.json(items);
	}),
];
