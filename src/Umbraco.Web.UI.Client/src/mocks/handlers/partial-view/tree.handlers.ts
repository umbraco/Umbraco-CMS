const { http, HttpResponse } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbPartialViewMockDB.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const parentPath = new URL(request.url).searchParams.get('parentPath');
		if (!parentPath) return new HttpResponse(null, { status: 400 });
		const skip = Number(new URL(request.url).searchParams.get('skip'));
		const take = Number(new URL(request.url).searchParams.get('take'));
		const response = umbPartialViewMockDB.tree.getChildrenOf({ parentPath, skip, take });
		return HttpResponse.json(response);
	}),
];
