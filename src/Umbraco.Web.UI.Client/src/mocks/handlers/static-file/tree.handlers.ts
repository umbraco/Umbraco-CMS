const { rest } = window.MockServiceWorker;
import { umbStaticFileMockDb } from '../../data/static-file/static-file.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const response = umbStaticFileMockDb.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const parentPath = req.url.searchParams.get('parentPath');
		if (!parentPath) return res(ctx.status(400));
		const response = umbStaticFileMockDb.tree.getChildrenOf(parentPath);
		return res(ctx.status(200), ctx.json(response));
	}),
];
