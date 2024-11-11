const { rest } = window.MockServiceWorker;
import { umbMemberGroupMockDb } from '../../data/member-group/member-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const filterHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/filter`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const filter = req.url.searchParams.get('filter');

		const options = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const response = umbMemberGroupMockDb.filter(options);
		return res(ctx.status(200), ctx.json(response));
	}),
];
