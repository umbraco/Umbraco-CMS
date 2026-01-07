const { http, HttpResponse } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	http.get(umbracoPath(`/filter${UMB_SLUG}`), ({ request }) => {
		const url = new URL(request.url);
		const skip = Number(url.searchParams.get('skip'));
		const take = Number(url.searchParams.get('take'));
		const orderBy = url.searchParams.get('orderBy');
		const orderDirection = url.searchParams.get('orderDirection');
		const memberGroupIds = url.searchParams.getAll('memberGroupIds');
		const memberTypeId = url.searchParams.get('memberTypeId');
		const filter = url.searchParams.get('filter');

		const options: any = {
			skip: skip || undefined,
			take: take || undefined,
			orderBy: orderBy || undefined,
			orderDirection: orderDirection || undefined,
			memberGroupIds: memberGroupIds.length > 0 ? memberGroupIds : undefined,
			memberTypeId: memberTypeId || undefined,
			filter: filter || undefined,
		};

		const response = umbMemberMockDb.filter(options);
		return HttpResponse.json(response);
	}),
];
