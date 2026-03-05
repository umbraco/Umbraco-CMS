const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { ProfilingStatusResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

export const handlers = [
	http.get(umbracoPath('/profiling/status'), () => {
		return HttpResponse.json<ProfilingStatusResponseModel>({ enabled: true });
	}),

	http.put(umbracoPath('/profiling/status'), () => {
		return new HttpResponse(null, { status: 200 });
	}),
];
