const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { PagedSegmentResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/segment'), () => {
		return HttpResponse.json<PagedSegmentResponseModel>({
			total: 0,
			items: [],
		});
	}),
];
