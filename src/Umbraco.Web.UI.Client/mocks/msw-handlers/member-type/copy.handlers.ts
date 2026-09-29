const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberTypeMockDb } from '../../db/member-type.db.js';
import { UMB_SLUG } from './slug.js';
import type { CopyMemberTypeRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const copyHandlers = [
	http.post(umbracoPath(`${UMB_SLUG}/:id/copy`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400, statusText: 'no id found' });

		const requestBody = (await request.json()) as CopyMemberTypeRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		// A missing/null target means "copy to the tree root" — target isn't required.
		const newIds = umbMemberTypeMockDb.tree.copy([id], requestBody.target?.id ?? null);

		return new HttpResponse(null, { status: 201, headers: { Location: newIds[0] } });
	}),
];
