const { rest } = window.MockServiceWorker;
import { umbDictionaryMockDb } from '../../data/dictionary/dictionary.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const response = umbDictionaryMockDb.tree.getRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const response = umbDictionaryMockDb.tree.getChildrenOf(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),
];
