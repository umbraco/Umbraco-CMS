const { http, HttpResponse } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`/filter${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const orderBy = url.searchParams.get('orderBy');
		const orderDirection = url.searchParams.get('orderDirection');
		const userGroupIds = url.searchParams.getAll('userGroupIds');
		const userStates = url.searchParams.getAll('userStates');
		const filter = url.searchParams.get('filter');

		const options: any = {
			skip: skip || undefined,
			take: take || undefined,
			orderBy: orderBy || undefined,
			orderDirection: orderDirection || undefined,
			userGroupIds: userGroupIds.length > 0 ? userGroupIds : undefined,
			userStates: userStates.length > 0 ? userStates : undefined,
			filter: filter || undefined,
		};

		const response = umbUserMockDb.filter(options);
		return HttpResponse.json(response);
	}),
];
