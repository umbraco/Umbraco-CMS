const { rest } = window.MockServiceWorker;
import { umbStylesheetData } from '../../data/stylesheet/stylesheet.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const response = umbStylesheetData.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const path = req.url.searchParams.get('path');
		if (!path) return res(ctx.status(400));
		const response = umbStylesheetData.tree.getChildrenOf(path);
		return res(ctx.status(200), ctx.json(response));
	}),
];
