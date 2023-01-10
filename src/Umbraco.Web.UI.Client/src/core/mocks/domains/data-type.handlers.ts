import { rest } from 'msw';
import { umbDataTypeData } from '../data/data-type.data';
import type { DataTypeDetails } from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/data-type/details/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([dataType]));
	}),

	rest.post<DataTypeDetails[]>('/umbraco/backoffice/data-type/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<string[]>('/umbraco/backoffice/data-type/delete', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		umbDataTypeData.delete(keys);

		return res(ctx.status(200));
	}),

	rest.get('/umbraco/management/api/v1/tree/data-type/root', (req, res, ctx) => {
		const rootItems = umbDataTypeData.getTreeRoot();
		const response = {
			total: rootItems.length,
			items: rootItems,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/data-type/children', (req, res, ctx) => {
		const parentKey = req.url.searchParams.get('parentKey');
		if (!parentKey) return;

		const children = umbDataTypeData.getTreeItemChildren(parentKey);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/data-type/item', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;
		const items = umbDataTypeData.getTreeItem(keys);
		return res(ctx.status(200), ctx.json(items));
	}),
];
