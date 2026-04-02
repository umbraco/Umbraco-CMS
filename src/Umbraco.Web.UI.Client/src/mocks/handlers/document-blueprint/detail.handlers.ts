const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../data/document-blueprint/document-blueprint.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbDocumentBlueprintMockDb.detail.create(requestBody);

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
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbDocumentBlueprintMockDb.detail.read(id);
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbDocumentBlueprintMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbDocumentBlueprintMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
