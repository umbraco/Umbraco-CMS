import { rest } from 'msw';
import { umbDocumentTypeData } from '../data/document-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/tree/document-type/root', (req, res, ctx) => {
		const rootItems = umbDocumentTypeData.getTreeRoot();
		const response = {
			total: rootItems.length,
			items: rootItems,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document-type/children', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const children = umbDocumentTypeData.getTreeItemChildren(key);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document-type/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbDocumentTypeData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
