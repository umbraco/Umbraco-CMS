const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserGroupMockDb } from '../../data/user-group/user-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const ids = searchParams.getAll('id');
		if (!ids) return;
		const items = umbUserGroupMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
