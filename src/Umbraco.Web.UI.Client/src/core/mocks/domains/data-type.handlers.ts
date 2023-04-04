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
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;

		const children = umbDataTypeData.getTreeItemChildren(parentId);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/tree/data-type/item'), (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbDataTypeData.getTreeItem(ids);
		return res(ctx.status(200), ctx.json(items));
	}),

	// FOLDERS
	rest.post(umbracoPath('/data-type/folder'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.createFolder(data);

		return res(ctx.status(200));
	}),

	rest.get(umbracoPath('/data-type/folder/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dataType = umbDataTypeData.getById(id);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath('/data-type/folder/:id'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDataTypeData.save(data);

		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/data-type/folder/:id'), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		try {
			umbDataTypeData.deleteFolder(id);
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

	rest.get(umbracoPath('/data-type/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const dataType = umbDataTypeData.getById(id);

		return res(ctx.status(200), ctx.json(dataType));
	}),

	rest.put(umbracoPath('/data-type/:id'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.delete<string>(umbracoPath('/data-type/:id'), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbDataTypeData.delete([id]);

		return res(ctx.status(200));
	}),
];
