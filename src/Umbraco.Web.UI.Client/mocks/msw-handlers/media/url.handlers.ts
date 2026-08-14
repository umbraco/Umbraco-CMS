const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../db/media.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const urlHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/urls`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids.length) return new HttpResponse(null, { status: 400 });

		return HttpResponse.json(umbMediaMockDb.getFileUrls(ids));
	}),
];
