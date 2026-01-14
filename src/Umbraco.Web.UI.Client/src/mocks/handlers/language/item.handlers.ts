const { http, HttpResponse } = window.MockServiceWorker;
import { umbLanguageMockDb } from '../../data/language/language.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const itemHandlers = [
	http.get(umbracoPath(`/item${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const isoCodes = url.searchParams.getAll('isoCode');
		if (!isoCodes) return;
		const items = umbLanguageMockDb.item.getItems(isoCodes);
		return HttpResponse.json(items);
	}),
];
