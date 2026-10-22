const { http, HttpResponse } = window.MockServiceWorker;

import { umbMockManager } from '../mock-manager.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedSegmentResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/segment'), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number.parseInt(url.searchParams.get('skip') ?? '0');
		const takeParam = url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const segments = umbMockManager.getDataSet().segment ?? [];
		const items = segments.slice(skip, take !== undefined ? skip + take : undefined);

		return HttpResponse.json<PagedSegmentResponseModel>({
			total: segments.length,
			items,
		});
	}),
];
