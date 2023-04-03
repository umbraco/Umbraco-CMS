import { rest } from 'msw';
import { umbMediaTypeData } from '../data/media-type.data';

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
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbMediaTypeData.getTreeItem(keys);
		return res(ctx.status(200), ctx.json(items));
	}),
];
