const { http, HttpResponse } = window.MockServiceWorker;

import { umbMockManager } from '../mock-manager.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedSegmentResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/segment'), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = Number(searchParams.get('skip') ?? 0);
		const take = Number(searchParams.get('take') ?? 100);
		const segments = umbMockManager.getDataSet().segment ?? [];

		return HttpResponse.json<PagedSegmentResponseModel>({
			total: segments.length,
			items: segments.slice(skip, skip + take),
		});
	}),
];
