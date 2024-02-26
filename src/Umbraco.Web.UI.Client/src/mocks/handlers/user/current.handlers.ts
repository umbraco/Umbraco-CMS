const { rest } = window.MockServiceWorker;
import { umbUserMockDb } from '../../data/user/user.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath(`${UMB_SLUG}/current`), (_req, res, ctx) => {
		const loggedInUser = umbUserMockDb.getCurrentUser();
		return res(ctx.status(200), ctx.json(loggedInUser));
	}),
];
