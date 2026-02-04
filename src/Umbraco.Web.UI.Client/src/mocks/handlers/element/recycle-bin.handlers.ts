const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../data/element/element.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const recycleBinHandlers = [
	http.get(umbracoPath(`/recycle-bin${UMB_SLUG}/root`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbElementMockDb.recycleBin.tree.getRoot({ skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/recycle-bin${UMB_SLUG}/children`), ({ request }) => {
		const url = new URL(request.url);
		const parentId = url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const response = umbElementMockDb.recycleBin.tree.getChildrenOf({ parentId, skip, take });
		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/recycle-bin${UMB_SLUG}/siblings`), ({ request }) => {
		const url = new URL(request.url);
		const id = url.searchParams.get('id');
		if (!id) return new HttpResponse(null, { status: 400 });

		const item = umbElementMockDb.recycleBin.read(id);
		if (!item) return new HttpResponse(null, { status: 404 });

		const parentId = item.parent?.id;
		const skip = Number(url.searchParams.get('skip') ?? 0);
		const take = Number(url.searchParams.get('take') ?? 100);

		let siblings;
		if (parentId) {
			siblings = umbElementMockDb.recycleBin.tree.getChildrenOf({ parentId, skip, take });
		} else {
			siblings = umbElementMockDb.recycleBin.tree.getRoot({ skip, take });
		}

		return HttpResponse.json({
			total: siblings.total,
			items: siblings.items,
		});
	}),

	http.put(umbracoPath(`${UMB_SLUG}/:id/move-to-recycle-bin`), async ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbElementMockDb.recycleBin.trash([id]);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`/recycle-bin${UMB_SLUG}/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbElementMockDb.recycleBin.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`/recycle-bin${UMB_SLUG}`), () => {
		umbElementMockDb.recycleBin.emptyRecycleBin();
		return new HttpResponse(null, { status: 200 });
	}),

	http.delete(umbracoPath(`/recycle-bin${UMB_SLUG}/folder/:id`), ({ params }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });
		umbElementMockDb.recycleBin.delete(id);
		return new HttpResponse(null, { status: 200 });
	}),
];
