const { http, HttpResponse } = window.MockServiceWorker;

import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath('/published-cache/status'), () => {
		return HttpResponse.json<string>(
			'Database cache is ok. ContentStore contains 1 item and has 1 generation and 0 snapshot. MediaStore contains 5 items and has 1 generation and 0 snapshot.',
		);
	}),

	http.post(umbracoPath('/published-cache/reload'), async () => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return new HttpResponse(null, { status: 200 });
	}),

	http.post(umbracoPath('/published-cache/rebuild'), async () => {
		await new Promise((resolve) => setTimeout(resolve, (Math.random() + 1) * 1000)); // simulate a delay of 1-2 seconds

		return new HttpResponse(null, { status: 200 });
	}),

	http.post(umbracoPath('/published-cache/collect'), () => {
		return new HttpResponse(null, { status: 200 });
	}),
];
