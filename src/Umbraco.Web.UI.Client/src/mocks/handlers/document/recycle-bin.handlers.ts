const { http, HttpResponse } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const recycleBinHandlers = [
	http.get(umbracoPath(`/recycle-bin${UMB_SLUG}/root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbDocumentMockDb.recycleBin.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/recycle-bin${UMB_SLUG}/children`), ({ request }) => {
		const url = new URL(request.url);
		const parentId = url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbDocumentMockDb.recycleBin.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/move-to-recycle-bin`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbDocumentMockDb.recycleBin.trash([id]);
		return new HttpResponse(null, { status: 200 });
	}),
];
