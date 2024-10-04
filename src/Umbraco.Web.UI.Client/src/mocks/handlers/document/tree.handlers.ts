const { rest } = window.MockServiceWorker;
import { umbDocumentMockDb } from '../../data/document/document.db.js';
import { UMB_SLUG } from './slug.js';
import type { GetTreeDocumentAncestorsResponse } from '@umbraco-cms/backoffice/external/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const treeHandlers = [
	rest.get(umbracoPath(`/tree${UMB_SLUG}/root`), (req, res, ctx) => {
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbDocumentMockDb.tree.getRoot({ skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/children`), (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const skip = Number(req.url.searchParams.get('skip'));
		const take = Number(req.url.searchParams.get('take'));
		const response = umbDocumentMockDb.tree.getChildrenOf({ parentId, skip, take });
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath(`/tree${UMB_SLUG}/ancestors`), (req, res, ctx) => {
		const descendantId = req.url.searchParams.get('descendantId');
		if (!descendantId) return;
		const response = umbDocumentMockDb.tree.getAncestorsOf({ descendantId });
		return res(ctx.status(200), ctx.json<GetTreeDocumentAncestorsResponse>(response));
	}),
];
