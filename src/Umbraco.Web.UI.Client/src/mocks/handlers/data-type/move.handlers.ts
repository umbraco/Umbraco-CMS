const { http, HttpResponse } = window.MockServiceWorker;
import { umbDataTypeMockDb } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import type { MoveDataTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveHandlers = [
	http.put(umbracoPath(`${UMB_SLUG}/:id/move`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const requestBody = (await request.json()) as MoveDataTypeRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		if (!requestBody.target?.id) return new HttpResponse(null, { status: 400, statusText: 'no targetId found' });

		umbDataTypeMockDb.tree.move([id], requestBody.target.id);
		return new HttpResponse(null, { status: 200 });
	}),
];
