const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../data/media/media.db.js';
import { items as referenceData } from '../../data/tracked-reference.data.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateMediaRequestModel,
	PagedIReferenceResponseModel,
	UpdateMediaRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { UmbMediaDetailModel } from '@umbraco-cms/backoffice/media';

export const detailHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}`), async ({ request }) => {
		const requestBody = (await request.json()) as CreateMediaRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		const id = umbMediaMockDb.detail.create(requestBody);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + id,
				'Umb-Generated-Resource': id,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/referenced-by`), ({ params }) => {
		const id = params.id as string;
		if (!id) return;
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}

		const PagedTrackedReference = {
			total: referenceData.length,
			items: referenceData,
		};

		return HttpResponse.json<PagedIReferenceResponseModel>(PagedTrackedReference);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbMediaMockDb.detail.read(id);
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/validate`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const model = (await request.json()) as UmbMediaDetailModel;
		if (!model) return new HttpResponse(null, { status: 400 });

		const hasMediaPickerOrFileUploadValue = model.values.some((v) => {
			return v.editorAlias === 'Umbraco.UploadField' && v.value;
		});

		if (!hasMediaPickerOrFileUploadValue) {
			return new HttpResponse(null, { status: 400, statusText: 'No media picker or file upload value found' });
		}

		return new HttpResponse(null, { status: 200 });
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateMediaRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		umbMediaMockDb.detail.update(id, requestBody);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		if (id === 'forbidden') {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbMediaMockDb.detail.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
