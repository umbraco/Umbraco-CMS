const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberGroupMockDb } from '../../data/member-group/member-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const filterHandlers = [
	http.get(umbracoPath(`${UMB_SLUG}/filter`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const filter = url.searchParams.get('filter');

		const options = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const response = umbMemberGroupMockDb.filter(options);
		return HttpResponse.json(response);
	}),
];
