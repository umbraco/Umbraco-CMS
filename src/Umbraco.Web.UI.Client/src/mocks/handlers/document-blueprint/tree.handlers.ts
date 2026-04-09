const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentBlueprintMockDb } from '../../data/document-blueprint/document-blueprint.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbDocumentBlueprintMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const parentId = new URL(request.url).searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbDocumentBlueprintMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),
];
