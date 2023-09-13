const { rest } = window.MockServiceWorker;
import { umbDocumentData } from '../../data/document.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

export const handlers = [
	rest.get(umbracoPath('/tree/document/root'), (req, res, ctx) => {
		const response = umbDocumentData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/document/children'), (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const response = umbDocumentData.getTreeItemChildrenOf(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),
];
