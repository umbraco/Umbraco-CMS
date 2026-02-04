const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../data/element/element.db.js';
import { items as referenceData } from '../../data/tracked-reference.data.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateElementRequestModel,
	DefaultReferenceResponseModel,
	PagedIReferenceResponseModel,
	UpdateElementRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

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

	http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
		return HttpResponse.json(umbElementMockDb.getConfiguration());
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		const url = new URL(request.url);
		const query = url.searchParams;
		const skip = query.get('skip') ? parseInt(query.get('skip') as string, 10) : 0;
		const take = query.get('take') ? parseInt(query.get('take') as string, 10) : 100;

		let data: Array<DefaultReferenceResponseModel> = [];

		if (id === 'all-property-editors-document-id') {
			data = referenceData;
		}

		const PagedTrackedReference: PagedIReferenceResponseModel = {
			total: data.length,
			items: data.slice(skip, skip + take),
		};

		return HttpResponse.json(PagedTrackedReference);
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
