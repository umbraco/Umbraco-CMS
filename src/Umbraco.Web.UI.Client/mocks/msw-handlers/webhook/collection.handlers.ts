const { http, HttpResponse } = window.MockServiceWorker;
import { umbWebhookMockDb } from '../../db/webhook.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const skipParam = url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const allItems = umbWebhookMockDb.getAll();
		const start = skip ?? 0;
		const end = start + (take ?? 100);
		const paged = allItems.slice(start, end);

		const items = paged.map((item) => ({
			id: item.id,
			name: item.name,
			description: item.description,
			url: item.url,
			enabled: item.enabled,
			events: item.events,
			contentTypeKeys: item.contentTypeKeys,
			headers: item.headers,
		}));

		return HttpResponse.json({ items, total: allItems.length });
	}),
];
