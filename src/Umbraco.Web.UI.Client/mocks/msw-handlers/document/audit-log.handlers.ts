const { http, HttpResponse } = window.MockServiceWorker;
import { umbMockManager } from '../../mock-manager.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const auditLogHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/:id/audit-log`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip')) || 0;
		const take = Number(url.searchParams.get('take')) || 100;

		const allLogs = umbMockManager.getDataSet().auditLogs ?? [];
		const paged = allLogs.slice(skip, skip + take);

		return HttpResponse.json({ items: paged, total: allLogs.length });
	}),
];
