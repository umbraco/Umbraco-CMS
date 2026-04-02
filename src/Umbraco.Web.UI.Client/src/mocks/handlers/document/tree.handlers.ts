const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import type { GetTreeDocumentAncestorsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbDocumentMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const url = new URL(request.url);
		const parentId = url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbDocumentMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/ancestors`), ({ request }) => {
		const url = new URL(request.url);
		const descendantId = url.searchParams.get('descendantId');
		if (!descendantId) return;
		const response = umbDocumentMockDb.tree.getAncestorsOf({ descendantId });
		return HttpResponse.json<GetTreeDocumentAncestorsResponse>(response);
	}),
];
