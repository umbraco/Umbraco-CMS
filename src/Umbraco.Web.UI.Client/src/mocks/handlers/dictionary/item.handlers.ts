const { http, HttpResponse } = window.MockServiceWorker;
import { umbDictionaryMockDb } from '../../data/dictionary/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const ids = searchParams.getAll('id');
		if (!ids) return;
		const items = umbDictionaryMockDb.item.getItems(ids);
		return HttpResponse.json(items);
	}),
];
