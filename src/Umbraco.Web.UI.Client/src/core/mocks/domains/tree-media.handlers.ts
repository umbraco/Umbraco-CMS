import { rest } from 'msw';
import { umbMediaData } from '../data/media.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/media/root', (req, res, ctx) => {
		const response = umbMediaData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/children', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;
		const response = umbMediaData.getTreeItemChildren(key);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbMediaData.getTreeItem(keys.split(','));

		return res(ctx.status(200), ctx.json(items));
	}),
];
