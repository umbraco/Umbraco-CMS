const { rest } = window.MockServiceWorker;
import { umbUserGroupMockDb } from '../../data/user-group/user-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`/filter${UMB_SLUG}`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const filter = req.url.searchParams.get('filter');

		const options: any = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const response = umbUserGroupMockDb.filter(options);
		return res(ctx.status(200), ctx.json(response));
	}),
];
