const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../db/document.db.js';
import { UMB_SLUG } from './slug.js';
import type { CopyDocumentRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const copyHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/:id/copy`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400, statusText: 'no id found' });

		const requestBody = (await request.json()) as CopyDocumentRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		// A missing/null target means "copy to the tree root" — target isn't required.
		// `relateToOriginal`/`includeDescendants` aren't simulated by this mock.
		const newIds = umbDocumentMockDb.tree.copy([id], requestBody.target?.id ?? null);

		return new HttpResponse(null, { status: 201, headers: { Location: newIds[0] } });
	}),
];
