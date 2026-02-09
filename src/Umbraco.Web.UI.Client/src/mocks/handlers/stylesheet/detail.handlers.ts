const { http, HttpResponse } = window.MockServiceWorker;
import { createProblemDetails } from '../../data/utils.js';
import { umbStylesheetMockDb } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import type {
	CreateStylesheetRequestModel,
	UpdateStylesheetRequestModel,
} from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const detailHandlers = [
	http.post(umbracoPath(UMB_SLUG), async ({ request }) => {
		const requestBody = (await request.json()) as CreateStylesheetRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });
		const path = umbStylesheetMockDb.file.create(requestBody);
		const encodedPath = encodeURIComponent(path);

		// Validate name
		if (!requestBody.name) {
			return HttpResponse.json(createProblemDetails({ title: 'Validation', detail: 'name is required' }), {
				status: 400,
			});
		}

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:path`), ({ params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });
		if (path.endsWith('forbidden')) {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const response = umbStylesheetMockDb.file.read(decodeURIComponent(path));
		return HttpResponse.json(response);
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/:path`), ({ params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });
		if (path.endsWith('forbidden')) {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		umbStylesheetMockDb.file.delete(decodeURIComponent(path));
		return new HttpResponse(null, { status: 200 });
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:path`), async ({ request, params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });
		if (path.endsWith('forbidden')) {
			// Simulate a forbidden response
			return new HttpResponse(null, { status: 403 });
		}
		const requestBody = (await request.json()) as UpdateStylesheetRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });
		umbStylesheetMockDb.file.update(decodeURIComponent(path), requestBody);
		return new HttpResponse(null, { status: 200 });
	}),
];
