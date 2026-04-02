const { http, HttpResponse } = window.MockServiceWorker;
import { umbDictionaryMockDb } from '../../data/dictionary/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateDictionaryItemRequestModel,
	PagedDictionaryOverviewResponseModel,
	UpdateDictionaryItemRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateDictionaryItemRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbDictionaryMockDb.detail.create(requestBody);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}`), () => {
		const response: PagedDictionaryOverviewResponseModel = umbDictionaryMockDb.getOverview();
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbDictionaryMockDb.detail.read(id);
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateDictionaryItemRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbDictionaryMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbDictionaryMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
