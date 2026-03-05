const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaTypeMockDb } from '../../db/media-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbMediaTypeMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const parentId = new URL(request.url).searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbMediaTypeMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/ancestors`), ({ request }) => {
		const id = new URL(request.url).searchParams.get('descendantId');
		if (!id) return;
		const response = umbMediaTypeMockDb.tree.getAncestorsOf({ descendantId: id });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/siblings`), ({ request }) => {
		const url = new URL(request.url);
		const targetId = url.searchParams.get('target');
		if (!targetId) return;
		const before = Number(url.searchParams.get('before'));
		const after = Number(url.searchParams.get('after'));
		const response = umbMediaTypeMockDb.tree.getSiblingsOf({ targetId, before, after });
		return HttpResponse.json(response);
	}),
];
