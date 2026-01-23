const { http, HttpResponse } = window.MockServiceWorker;
import { umbDataTypeMockDb } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import type { CopyDataTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const copyHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/:id/copy`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400, statusText: 'no id found' });

		const requestBody = (await request.json()) as CopyDataTypeRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });
		if (!requestBody.target?.id) return new HttpResponse(null, { status: 400, statusText: 'no targetId found' });

		const newIds = umbDataTypeMockDb.tree.copy([id], requestBody.target.id);

		return new HttpResponse(null, { status: 201, headers: { Location: newIds[0] } });
	}),
];
