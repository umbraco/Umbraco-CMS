import { rest } from 'msw';
import { umbDocumentData } from '../data/document.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/document/root', (req, res, ctx) => {
		const response = umbDocumentData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/children', (req, res, ctx) => {
		const parentKey = req.url.searchParams.get('parentKey');
		if (!parentKey) return;
		const response = umbDocumentData.getTreeItemChildren(parentKey);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/item', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbDocumentData.getTreeItems(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
