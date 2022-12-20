import { rest } from 'msw';
import { umbMediaTypeData } from '../data/media-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/media-type/root', (req, res, ctx) => {
		const response = umbMediaTypeData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media-type/children', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const response = umbMediaTypeData.getTreeItemChildren(key);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media-type/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbMediaTypeData.getTreeItem(keys.split(','));
		return res(ctx.status(200), ctx.json(items));
	}),
];
