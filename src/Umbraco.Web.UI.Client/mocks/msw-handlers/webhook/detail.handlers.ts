const { http, HttpResponse } = window.MockServiceWorker;
import { umbWebhookMockDb } from '../../db/webhook.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || undefined;
		const take = Number(url.searchParams.get('take')) || undefined;

		return HttpResponse.json(umbWebhookMockDb.detail.list({ skip, take }));
	}),

	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as any;
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		const id = umbWebhookMockDb.detail.create(requestBody);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const response = umbWebhookMockDb.detail.read(id);
		if (!response) return new HttpResponse(null, { status: 404 });

		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const requestBody = (await request.json()) as any;
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		umbWebhookMockDb.detail.update(id, requestBody);

		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		umbWebhookMockDb.detail.delete(id);

		return new HttpResponse(null, { status: 200 });
	}),
];
