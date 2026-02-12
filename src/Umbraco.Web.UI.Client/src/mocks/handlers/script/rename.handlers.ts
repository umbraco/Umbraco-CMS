const { http, HttpResponse } = window.MockServiceWorker;
import { umbScriptMockDb } from '../../data/script/script.db.js';
import { UMB_SLUG } from './slug.js';
import type { RenameStylesheetRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const renameHandlers = [
	http.put(umbracoPath(`${UMB_SLUG}/:path/rename`), async ({ request, params }) => {
		const path = params.path as string;
		if (!path) return new HttpResponse(null, { status: 400 });

		const requestBody = (await request.json()) as RenameStylesheetRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400 });

		const newPath = umbScriptMockDb.file.rename(decodeURIComponent(path), requestBody.name);
		const encodedPath = encodeURIComponent(newPath);

		return new HttpResponse(null, {
			status: 201,
			headers: {
				Location: request.url + '/' + encodedPath,
				'Umb-Generated-Resource': encodedPath,
			},
		});
	}),
];
