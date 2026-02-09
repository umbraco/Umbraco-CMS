const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserGroupMockDb } from '../../data/user-group/user-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`/filter${UMB_SLUG}`), ({ request }) => {
		const searchParams = new URL(request.url).searchParams;
		const skip = Number(searchParams.get('skip'));
		const take = Number(searchParams.get('take'));
		const filter = searchParams.get('filter');

		const options: any = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const response = umbUserGroupMockDb.filter(options);
		return HttpResponse.json(response);
	}),
];
