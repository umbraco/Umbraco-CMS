const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../data/element/element.db.js';
import { UMB_SLUG } from './slug.js';
import type { CopyElementRequestModel, MoveElementRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveCopyHandlers = [
	http.put<{ id: string }, MoveElementRequestModel>(
		umbracoPath(`${UMB_SLUG}/:id/move`),
		async ({ request, params }) => {
			const id = params.id;
			if (!id) return new HttpResponse(null, { status: 400 });
			if (id === 'forbidden') {
				return new HttpResponse(null, { status: 403 });
			}
			const requestBody = await request.json();
			if (!requestBody) return new HttpResponse(null, { status: 400 });
			umbElementMockDb.tree.move([id], requestBody.target?.id ?? '');
			return new HttpResponse(null, { status: 200 });
		},
	),

	http.post<{ id: string }, CopyElementRequestModel>(
		umbracoPath(`${UMB_SLUG}/:id/copy`),
		async ({ request, params }) => {
			const id = params.id;
			if (!id) return new HttpResponse(null, { status: 400 });
			if (id === 'forbidden') {
				return new HttpResponse(null, { status: 403 });
			}
			const requestBody = await request.json();
			if (!requestBody) return new HttpResponse(null, { status: 400 });
			const newIds = umbElementMockDb.tree.copy([id], requestBody.target?.id ?? '');
			const newId = newIds[0];
			return new HttpResponse(null, {
				status: 201,
				headers: {
					Location: request.url.replace(`/${id}/copy`, `/${newId}`),
					'Umb-Generated-Resource': newId,
				},
			});
		},
	),
];
