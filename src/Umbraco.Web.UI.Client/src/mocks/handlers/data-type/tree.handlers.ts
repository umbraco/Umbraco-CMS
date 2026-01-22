const { http, HttpResponse } = window.MockServiceWorker;
import { umbDataTypeMockDb } from '../../data/data-type/data-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = Number(searchParams.get('skip'));
		const take = Number(searchParams.get('take'));
		const response = umbDataTypeMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const parentId = searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(searchParams.get('skip'));
		const take = Number(searchParams.get('take'));
		const response = umbDataTypeMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/ancestors`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const descendantId = searchParams.get('descendantId');
		if (!descendantId) return;
		const response = umbDataTypeMockDb.tree.getAncestorsOf({ descendantId });
		return HttpResponse.json(response);
	}),
];
