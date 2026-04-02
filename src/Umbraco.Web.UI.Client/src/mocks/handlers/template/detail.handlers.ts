const { http, HttpResponse } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbTemplateMockDb } from '../../data/template/template.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateTemplateRequestModel,
	UpdateTemplateRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateTemplateRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		// Validate name and alias
		if (!requestBody.name || !requestBody.alias) {
			return HttpResponse.json(createProblemDetails({ title: 'Validation', detail: 'name and alias are required' }), {
				status: 400,
			});
		}

		const id = umbTemplateMockDb.detail.create(requestBody);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/scaffold`), () => {
		const response = umbTemplateMockDb.detail.createScaffold();
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbTemplateMockDb.detail.read(id);
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateTemplateRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		// Validate name and alias
		if (!requestBody.name || !requestBody.alias) {
			return HttpResponse.json(createProblemDetails({ title: 'Validation', detail: 'name and alias are required' }), {
				status: 400,
			});
		}

		umbTemplateMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbTemplateMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
