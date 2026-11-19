const { http, HttpResponse } = window.MockServiceWorker;
import type { UmbMockDocumentModel } from '../../data/mock-data-set.types.js';
import { umbMockManager } from '../../mock-manager.js';
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateAndPublishDocumentRequestModel,
	CreateDocumentRequestModel,
	GetDocumentByIdAvailableSegmentOptionsResponse,
	GetDocumentByIdReferencedDescendantsResponse,
	UpdateAndPublishDocumentRequestModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { getAreReferencedPage, getReferencedByPage, getReferencedDescendantsPage } from '../referenced-by.utils.js';

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

	http.get(umbracoPath(`${UMB_SLUG}/are-referenced`), ({ request }) => {
		return HttpResponse.json(getAreReferencedPage(request));
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		return HttpResponse.json(
			getReferencedByPage(id, request, () => (id === 'all-property-editors-document-id' ? getReferenceData() : [])),
		);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-descendants`), ({ request, params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		const ReferencedDescendantsResponse: GetDocumentByIdReferencedDescendantsResponse = getReferencedDescendantsPage(
			id,
			request,
			umbDocumentMockDb.getAll(),
		);

		return HttpResponse.json(ReferencedDescendantsResponse);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/available-segment-options`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		let document: UmbMockDocumentModel | null = null;

		try {
			document = umbDocumentMockDb.detail.read(id);
		} catch {
			return new HttpResponse(null, { status: 404 });
		}

		const availableSegments = document.variants.filter((v) => !!v.segment).map((v) => v.segment!) ?? [];

		const response: GetDocumentByIdAvailableSegmentOptionsResponse = {
			total: availableSegments.length,
			items: availableSegments.map((alias) => {
				// If the segment is generic (i.e. not tied to any culture) we show the segment on all cultures
				const isGeneric = alias.includes('generic');
				const whichCulturesHaveThisSegment: string[] | undefined = isGeneric
					? undefined
					: document.variants.filter((v) => v.segment === alias).map((v) => v.culture!);

				let availableSegmentOptions: string[] | null = whichCulturesHaveThisSegment ?? null;

				if (whichCulturesHaveThisSegment) {
					const hasNull = whichCulturesHaveThisSegment.some((c) => c === null);
					if (hasNull) {
						availableSegmentOptions = null;
					}
				}

				return {
					alias,
					name: `Segment: ${alias}`,
					cultures: availableSegmentOptions,
				};
			}),
		};

		return HttpResponse.json(response);
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
