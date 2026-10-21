const { http, HttpResponse } = window.MockServiceWorker;
import { umbMockManager } from '../../mock-manager.js';
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateAndPublishDocumentRequestModel,
	CreateDocumentRequestModel,
	GetDocumentByIdReferencedDescendantsResponse,
	PagedIReferenceResponseModel,
	UpdateAndPublishDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

function getReferenceData() {
	return umbMockManager.getDataSet().trackedReferenceItems ?? [];
}

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/validate`), () => {
		umbDocumentMockDb.detail.validate();
		return new HttpResponse(null, { status: 200 });
	}),

	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbDocumentMockDb.detail.create(requestBody);

		return HttpResponse.json(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.post(umbracoPath(`${UMB_SLUG}/create-and-publish`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateAndPublishDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbDocumentMockDb.publishing.createAndPublish(requestBody);

		return HttpResponse.json(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/configuration`), () => {
		return HttpResponse.json(umbDocumentMockDb.getConfiguration());
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

		let data: PagedIReferenceResponseModel['items'] = [];

		if (id === 'all-property-editors-document-id') {
			data = getReferenceData();
		}

		const PagedTrackedReference: PagedIReferenceResponseModel = {
			total: data.length,
			items: data.slice(skip, skip + take),
		};

		return HttpResponse.json(PagedTrackedReference);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-descendants`), ({ params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		const ReferencedDescendantsResponse: GetDocumentByIdReferencedDescendantsResponse = {
			total: 0,
			items: [],
		};

		return HttpResponse.json(ReferencedDescendantsResponse);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/validate`, 'v1.1'), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		umbDocumentMockDb.detail.validate(id);
		return new HttpResponse(null, { status: 200 });
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		try {
			const response = umbDocumentMockDb.detail.read(id);
			return HttpResponse.json(response);
		} catch {
			return new HttpResponse(null, { status: 404 });
		}
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/update-and-publish`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateAndPublishDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbDocumentMockDb.publishing.updateAndPublish(id, requestBody);
		return new HttpResponse(null, { status: 200 });
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
		umbDocumentMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbDocumentMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
