const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../data/element/element.db.js';
import { UMB_SLUG } from './slug.js';
import type { GetTreeElementAncestorsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	http.get(umbracoPath(`/tree${UMB_SLUG}/root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbElementMockDb.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/children`), ({ request }) => {
		const url = new URL(request.url);
		const parentId = url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbElementMockDb.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/ancestors`), ({ request }) => {
		const url = new URL(request.url);
		const descendantId = url.searchParams.get('descendantId');
		if (!descendantId) return;
		const response = umbElementMockDb.tree.getAncestorsOf({ descendantId });
		return HttpResponse.json<GetTreeElementAncestorsResponse>(response);
	}),

	http.get(umbracoPath(`/tree${UMB_SLUG}/siblings`), ({ request }) => {
		const url = new URL(request.url);
		const id = url.searchParams.get('id');
		if (!id) return new HttpResponse(null, { status: 400 });

		const item = umbElementMockDb.read(id);
		if (!item) return new HttpResponse(null, { status: 404 });

		const parentId = item.parent?.id;
		const skip = Number(url.searchParams.get('skip') ?? 0);
		const take = Number(url.searchParams.get('take') ?? 100);

		let siblings;
		if (parentId) {
			siblings = umbElementMockDb.tree.getChildrenOf({ parentId, skip, take });
		} else {
			siblings = umbElementMockDb.tree.getRoot({ skip, take });
		}

		return HttpResponse.json({
			total: siblings.total,
			items: siblings.items,
		});
	}),
];
