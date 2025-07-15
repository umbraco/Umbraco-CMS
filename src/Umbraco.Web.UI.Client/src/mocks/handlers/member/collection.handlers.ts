const { rest } = window.MockServiceWorker;
import { umbMemberMockDb } from '../../data/member/member.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	rest.get(umbracoPath(`/collection${UMB_SLUG}`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const filter = req.url.searchParams.get('filter');

		const options = {
			skip: skip || undefined,
			take: take || undefined,
			filter: filter || undefined,
		};

		const items = umbMemberMockDb.collection.getItems(options);
		return res(ctx.status(200), ctx.json(items));
	}),
];
