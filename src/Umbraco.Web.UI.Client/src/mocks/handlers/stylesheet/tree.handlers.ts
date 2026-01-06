const { http, HttpResponse } = window.MockServiceWorker;
import { umbStylesheetMockDb } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbStylesheetMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const parentPath = new URL(request.url).searchParams.get('parentPath');
		if (!parentPath) return new HttpResponse(null, { status: 400 });
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbStylesheetMockDb.tree.getChildrenOf({ parentPath, skip, take });
		return HttpResponse.json(response);
	}),
];
