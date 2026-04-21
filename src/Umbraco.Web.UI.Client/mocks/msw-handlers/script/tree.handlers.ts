const { http, HttpResponse } = window.MockServiceWorker;
import { umbScriptMockDb } from '../../db/script.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbScriptMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const parentPath = new URL(request.url).searchParams.get('parentPath');
		if (!parentPath) return new HttpResponse(null, { status: 400 });
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbScriptMockDb.tree.getChildrenOf({ parentPath, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/siblings`), ({ request }) => {
		const url = new URL(request.url);
		const path = url.searchParams.get('path');
		if (!path) return;
		const before = Number(url.searchParams.get('before'));
		const after = Number(url.searchParams.get('after'));
		const response = umbScriptMockDb.tree.getSiblingsOf({ path, before, after });
		return HttpResponse.json(response);
	}),
];
