const { rest } = window.MockServiceWorker;
import { umbPartialViewMockDB } from '../../data/partial-view/partial-view.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbPartialViewMockDB.tree.getRoot({ skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const parentPath = req.url.searchParams.get('parentPath');
		if (!parentPath) return res(ctx.status(400));
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbPartialViewMockDB.tree.getChildrenOf({ parentPath, skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),
];
