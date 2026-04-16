const { http, HttpResponse } = window.MockServiceWorker;
import { umbWebhookEventMockDb } from '../../db/webhook-event.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const eventsHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/events`), () => {
		return HttpResponse.json(umbWebhookEventMockDb.getAll());
	}),
];
