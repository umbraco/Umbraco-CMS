const { http, HttpResponse } = window.MockServiceWorker;
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UmbServertimeOffset } from '@umbraco-cms/backoffice/models';

export const handlers = [
	http.get(umbracoPath('/config/servertimeoffset'), () => {
		return HttpResponse.json<UmbServertimeOffset>({ offset: -120 });
	}),
];
