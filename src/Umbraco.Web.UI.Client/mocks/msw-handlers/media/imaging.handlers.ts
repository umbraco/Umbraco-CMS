const { http, HttpResponse } = window.MockServiceWorker;
import { umbImagingMockDb } from '../../db/imaging.db.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const imagingHandlers = [
	http.get(umbracoPath('/imaging/resize/urls'), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids.length) return new HttpResponse(null, { status: 400 });

		const params = {
			width: url.searchParams.get('width') ?? '200',
			height: url.searchParams.get('height') ?? '200',
			mode: url.searchParams.get('mode'),
			format: url.searchParams.get('format'),
		};

		return HttpResponse.json(umbImagingMockDb.getResizeUrls(ids, params));
	}),
];
