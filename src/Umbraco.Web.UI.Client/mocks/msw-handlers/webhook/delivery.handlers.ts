const { http, HttpResponse } = window.MockServiceWorker;
import { umbWebhookDeliveryMockDb } from '../../db/webhook-delivery.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const deliveryHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/logs`), ({ params, request }) => {
		const id = params.id as string;
		if (!id) return new HttpResponse(null, { status: 400 });

		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || undefined;
		const take = Number(url.searchParams.get('take')) || undefined;

		return HttpResponse.json(umbWebhookDeliveryMockDb.filter({ webhookId: id, skip, take }));
	}),
];
