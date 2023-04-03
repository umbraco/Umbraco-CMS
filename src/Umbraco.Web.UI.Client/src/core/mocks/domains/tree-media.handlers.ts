import { rest } from 'msw';
import { umbMediaData } from '../data/media.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/media/root', (req, res, ctx) => {
		const response = umbMediaData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const response = umbMediaData.getTreeItemChildren(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/item', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbMediaData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
