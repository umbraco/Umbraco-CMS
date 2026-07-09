const { http, HttpResponse } = window.MockServiceWorker;
import { umbAuditLogMockDb } from '../../db/audit-log.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const auditLogHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/audit-log`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || 0;
		const take = Number(url.searchParams.get('take')) || 100;

		return HttpResponse.json(umbAuditLogMockDb.get({ skip, take }));
	}),
];
