import { rest } from 'msw';
import { umbDataTypeData } from '../data/data-type.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import { ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';

// TODO: add schema
export const handlers = [
	// TREE
	rest.get(umbracoPath('/tree/data-type/root'), (req, res, ctx) => {
		const rootItems = umbDataTypeData.getTreeRoot();
		const response = {
			total: rootItems.length,
			items: rootItems,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/data-type/children'), (req, res, ctx) => {
		const parentKey = req.url.searchParams.get('parentKey');
		if (!parentKey) return;

		const children = umbDataTypeData.getTreeItemChildren(parentKey);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/data-type/item'), (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;
		const items = umbDataTypeData.getTreeItem(keys);
		return res(ctx.status(200), ctx.json(items));
	}),

	// FOLDERS
	rest.post(umbracoPath('/data-type/folder'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.createFolder(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/data-type/folder/:key'), (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath('/data-type/folder/:key'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.save(data);

		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/data-type/folder/:key'), async (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		try {
			umbDataTypeData.deleteFolder(key);
			return res(ctx.status(200));
		} catch (error) {
			return res(
				ctx.status(404),
				ctx.json<ProblemDetailsModel>({
					status: 404,
					type: 'error',
					detail: 'Not Found',
				})
			);
		}
	}),

	// Details
	rest.post(umbracoPath('/data-type'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.get(umbracoPath('/data-type/:key'), (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath('/data-type/:key'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.delete<string>(umbracoPath('/data-type/:key'), async (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		umbDataTypeData.delete([key]);

		return res(ctx.status(200));
	}),
];
