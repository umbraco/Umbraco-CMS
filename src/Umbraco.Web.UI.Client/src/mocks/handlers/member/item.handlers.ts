const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const ids = url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbMemberMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
