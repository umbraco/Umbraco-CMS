const { rest } = window.MockServiceWorker;
import { umbUserGroupMockDb } from '../../data/user-group/user-group.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const collectionHandlers = [
	rest.get(umbracoPath(`${UMB_SLUG}`), (req, res, ctx) => {
		const skipParam = req.url.searchParams.get('skip');
		const skip = skipParam ? Number.parseInt(skipParam) : undefined;
		const takeParam = req.url.searchParams.get('take');
		const take = takeParam ? Number.parseInt(takeParam) : undefined;

		const response = umbUserGroupMockDb.get({ skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),
];
