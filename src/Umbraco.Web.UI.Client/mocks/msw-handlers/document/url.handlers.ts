const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { GetDocumentUrlsResponse } from '@umbraco-cms/backoffice/external/backend-api';

export const urlHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/urls`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids.length) return new HttpResponse(null, { status: 400 });

		// When a culture is provided, variant URLs are restricted to that culture (invariant URLs, which have
		// no culture, are always kept). This mirrors the Management API's document URLs endpoint.
		const culture = url.searchParams.get('culture');

		const response: GetDocumentUrlsResponse = ids.map((id) => ({
			id,
			urlInfos: umbDocumentMockDb.url
				.getUrls(id)
				.filter((urlInfo) => !culture || urlInfo.culture === null || urlInfo.culture === culture)
				.map((urlInfo) => ({
					...urlInfo,
					message: null,
					provider: 'Default',
					isExternal: false,
				})),
		}));

		return HttpResponse.json(response);
	}),
];
