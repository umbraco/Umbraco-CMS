const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberTypeMockDb } from '../../data/member-type/member-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = Number(searchParams.get('skip') ?? '0');
		const take = Number(searchParams.get('take') ?? '100');
		const response = umbMemberTypeMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const parentId = searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(searchParams.get('skip') ?? '0');
		const take = Number(searchParams.get('take') ?? '100');
		const response = umbMemberTypeMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),
];
