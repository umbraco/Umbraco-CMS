const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentTypeMockDb } from '../../db/document-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const structureHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/allowed-children`), ({ params, request }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || 0;
		const take = Number(url.searchParams.get('take')) || 100;

		const response = umbDocumentTypeMockDb.getAllowedChildren(id, skip, take);
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/allowed-at-root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || 0;
		const take = Number(url.searchParams.get('take')) || 100;

		const response = umbDocumentTypeMockDb.getAllowedAtRoot(skip, take);
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`${UMB_SLUG}/:id/allowed-parents`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		const response = umbDocumentTypeMockDb.getAllowedParents(id);
		return HttpResponse.json(response);
	}),
];
