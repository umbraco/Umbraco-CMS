import { rest } from 'msw';
import { umbDataTypeData } from '../data/data-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/data-type/tree/root', (req, res, ctx) => {
		const rootItems = umbDataTypeData.getTreeRoot();
		const response = {
			total: rootItems.length,
			items: rootItems,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/data-type/tree/children', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const children = umbDataTypeData.getTreeItemChildren(key);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/data-type/tree/item', (req, res, ctx) => {
		const keys = req.params.keys as string;
		if (!keys) return;

		const items = umbDataTypeData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
