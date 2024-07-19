const { rest } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`/filter${UMB_SLUG}`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const orderBy = req.url.searchParams.get('orderBy');
		const orderDirection = req.url.searchParams.get('orderDirection');
		const userGroupIds = req.url.searchParams.getAll('userGroupIds');
		const userStates = req.url.searchParams.getAll('userStates');
		const filter = req.url.searchParams.get('filter');

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
		return res(ctx.status(200), ctx.json(response));
	}),
];
