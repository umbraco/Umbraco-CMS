const { rest } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`/filter${UMB_SLUG}`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const orderBy = req.url.searchParams.get('orderBy');
		const orderDirection = req.url.searchParams.get('orderDirection');
		const memberGroupIds = req.url.searchParams.getAll('memberGroupIds');
		const memberTypeId = req.url.searchParams.get('memberTypeId');
		const filter = req.url.searchParams.get('filter');

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
		return res(ctx.status(200), ctx.json(response));
	}),
];
