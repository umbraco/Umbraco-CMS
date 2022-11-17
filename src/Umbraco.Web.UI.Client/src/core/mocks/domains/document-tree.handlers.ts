import { rest } from 'msw';
import { umbDocumentData } from '../data/document.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/document/root', (req, res, ctx) => {
		const response = umbDocumentData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/children', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;
		const response = umbDocumentData.getTreeItemChildren(key);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbDocumentData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
