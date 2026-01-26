const { http, HttpResponse } = window.MockServiceWorker;
import { umbStaticFileMockDb } from '../../data/static-file/static-file.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbStaticFileMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const url = new URL(request.url);
		const parentPath = url.searchParams.get('parentPath');
		if (!parentPath) return new HttpResponse(null, { status: 400 });
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbStaticFileMockDb.tree.getChildrenOf({ parentPath, skip, take });
		return HttpResponse.json(response);
	}),
];
