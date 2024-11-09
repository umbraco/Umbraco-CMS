const { rest } = window.MockServiceWorker;
import { umbDocumentTypeMockDb } from '../../data/document-type/document-type.db.js';
import { UMB_SLUG } from './slug.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbDocumentTypeMockDb.tree.getRoot({ skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbDocumentTypeMockDb.tree.getChildrenOf({ parentId, skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),
];
