const { rest } = window.MockServiceWorker;
import { umbMediaTypeData } from '../data/media-type.data.js';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/media-type/root', (req, res, ctx) => {
		const response = umbMediaTypeData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media-type/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;

		const response = umbMediaTypeData.getTreeItemChildren(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media-type/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbMediaTypeData.getTreeItem(ids);
		return res(ctx.status(200), ctx.json(items));
	}),
];
