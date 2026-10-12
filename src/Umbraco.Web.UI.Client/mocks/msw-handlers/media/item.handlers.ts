const { http, HttpResponse } = window.MockServiceWorker;
import { umbMediaMockDb } from '../../db/media.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}/search`), ({ request }) => {
		const url = new URL(request.url);
		const query = url.searchParams.get('query') ?? '';
		const skip = Number(url.searchParams.get('skip')) || 0;
		const take = Number(url.searchParams.get('take')) || 100;

		const response = umbMediaMockDb.item.search(query, skip, take);

		return HttpResponse.json(response);
	}),

	// The picker's search results show the path to each hit, which it resolves through this endpoint. Without a
	// handler the request falls through to the dev server's index.html and search never resolves.
	http.get(umbracoPath(`/item${UMB_SLUG}/ancestors`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');

		const response = ids.map((id) => ({
			id,
			ancestors: umbMediaMockDb.item.getItems(
				umbMediaMockDb.tree.getAncestorsOf({ descendantId: id }).map((ancestor) => ancestor.id),
			),
		}));

		return HttpResponse.json(response);
	}),

	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMediaMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
