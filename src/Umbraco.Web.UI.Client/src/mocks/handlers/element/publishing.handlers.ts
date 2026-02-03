const { http, HttpResponse } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbElementMockDb } from '../../data/element/element.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	PublishElementRequestModel,
	UnpublishElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const publishingHandlers = [
	http.put(umbracoPath(`${UMB_SLUG}/:id/publish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const requestBody = (await request.json()) as PublishElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		try {
			umbElementMockDb.publishing.publish(id, requestBody);
			return new HttpResponse(null, { status: 200 });
		} catch (error) {
			if (error instanceof Error) {
				return HttpResponse.json(createProblemDetails({ title: 'Publish', detail: error.message }), { status: 400 });
			}
			throw new Error('An error occurred while publishing the element');
		}
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/unpublish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const requestBody = (await request.json()) as UnpublishElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbElementMockDb.publishing.unpublish(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),
];
