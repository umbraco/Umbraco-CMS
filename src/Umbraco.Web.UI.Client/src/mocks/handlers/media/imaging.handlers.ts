const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import type { GetImagingResizeUrlsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const imagingHandlers = [
	http.get(umbracoPath('/imaging/resize/urls'), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids) return new HttpResponse(null, { status: 404 });

		const media = umbMediaMockDb.getAll().filter((item) => ids.includes(item.id));

		const response: GetImagingResizeUrlsResponse = media.map((item) => ({
			id: item.id,
			// TODO: Figure out where to populate the `urlInfos` array, as the server's `MediaResponseModel` removed the deprecated `urls` property. [LK]
			urlInfos: [], // item.urls,
		}));

		return HttpResponse.json(response);
	}),
];
