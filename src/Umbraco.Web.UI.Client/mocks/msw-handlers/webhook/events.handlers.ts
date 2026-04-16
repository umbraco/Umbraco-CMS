const { http, HttpResponse } = window.MockServiceWorker;
import { umbWebhookEventMockDb } from '../../db/webhook-event.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const eventsHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/events`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || undefined;
		const take = Number(url.searchParams.get('take')) || undefined;
		return HttpResponse.json(umbWebhookEventMockDb.get({ skip, take }));
	}),
];
