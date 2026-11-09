const { http, HttpResponse } = window.MockServiceWorker;
import { umbElementMockDb } from '../../db/element.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbElementMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),

	http.get(umbracoPath(`/item${UMB_SLUG}/folder`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		const items = ids
			.map((id) => umbElementMockDb.read(id))
			.filter((model) => !!model?.isFolder)
			.map((model) => ({ id: model!.id, name: model!.name, flags: model!.flags ?? [] }));
		return HttpResponse.json(items);
	}),
];
