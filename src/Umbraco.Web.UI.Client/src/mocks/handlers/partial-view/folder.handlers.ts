const { http, HttpResponse } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import type { CreatePartialViewFolderRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const folderHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/folder`), async ({ request }) => {
		const requestBody = (await request.json()) as CreatePartialViewFolderRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });
		const path = umbPartialViewMockDB.folder.create(requestBody);
		const encodedPath = encodeURIComponent(path);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			},
		});
	}),

	http.get(umbracoPath(`${UMB_SLUG}/folder/:path`), ({ params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });
		const response = umbPartialViewMockDB.folder.read(decodeURIComponent(path));
		return HttpResponse.json(response);
	}),

	http.delete(umbracoPath(`${UMB_SLUG}/folder/:path`), ({ params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });
		umbPartialViewMockDB.folder.delete(decodeURIComponent(path));
		return new HttpResponse(null, { status: 200 });
	}),
];
