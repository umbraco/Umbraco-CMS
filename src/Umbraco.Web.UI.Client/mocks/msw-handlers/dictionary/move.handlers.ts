const { http, HttpResponse } = window.MockServiceWorker;
import { umbDictionaryMockDb } from '../../db/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import type { MoveDictionaryRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const moveHandlers = [
	http.put(umbracoPath(`${UMB_SLUG}/:id/move`), async ({ request, params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const requestBody = (await request.json()) as MoveDictionaryRequestModel;
		if (!requestBody) return new HttpResponse(null, { status: 400, statusText: 'no body found' });

		// A missing/null target means "move to the tree root" — target isn't required.
		umbDictionaryMockDb.tree.move([id], requestBody.target?.id ?? null);
		return new HttpResponse(null, { status: 200 });
	}),
];
