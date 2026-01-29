const { http, HttpResponse } = window.MockServiceWorker;
import type { UmbMockDocumentModel } from '../../data/sets/index.js';
import { dataSet } from '../../data/sets/index.js';
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateDocumentRequestModel,
	DefaultReferenceResponseModel,
	GetDocumentByIdAvailableSegmentOptionsResponse,
	GetDocumentByIdReferencedDescendantsResponse,
	PagedIReferenceResponseModel,
	UpdateDocumentRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const referenceData = dataSet.trackedReferenceItems ?? [];

export const detailHandlers = [
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

		return new HttpResponse(null, { status: 200 });
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbDocumentMockDb.detail.read(id);
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
