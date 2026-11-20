const { http, HttpResponse } = window.MockServiceWorker;
import { umbMockManager } from '../../mock-manager.js';
import { umbElementMockDb } from '../../db/element.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateAndPublishElementRequestModel,
	CreateElementRequestModel,
	PagedIReferenceResponseModel,
	PagedReferenceByIdModel,
	UpdateAndPublishElementRequestModel,
	UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { getAreReferencedPage, getReferencedByPage } from '../referenced-by.utils.js';

function getReferenceData() {
	return umbMockManager.getDataSet().trackedReferenceItems ?? [];
}

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbElementMockDb.detail.create(requestBody);

		return HttpResponse.json(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.post(umbracoPath(`${UMB_SLUG}/create-and-publish`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateAndPublishElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbElementMockDb.publishing.createAndPublish(requestBody);

		return HttpResponse.json(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
		return HttpResponse.json(umbElementMockDb.getConfiguration());
	}),

	http.get(umbracoPath(`${UMB_SLUG}/are-referenced`), ({ request }) => {
		const response: PagedReferenceByIdModel = getAreReferencedPage(request);

		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		const pagedTrackedReference: PagedIReferenceResponseModel = getReferencedByPage(id, request, () =>
			id === 'simple-element-id' ? getReferenceData() : [],
		);

		return HttpResponse.json(pagedTrackedReference);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbElementMockDb.detail.read(id);
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/update-and-publish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateAndPublishElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbElementMockDb.publishing.updateAndPublish(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbElementMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			return new HttpResponse(null, { status: 403 });
		}
		umbElementMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),

	http.post(umbracoPath(`${UMB_SLUG}/validate`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateElementRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		return new HttpResponse(null, { status: 200 });
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/validate`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			return new HttpResponse(null, { status: 403 });
		}
		return new HttpResponse(null, { status: 200 });
	}),
];
