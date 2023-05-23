const { rest } = window.MockServiceWorker;
import { umbRelationTypeData } from '../data/relation-type.data';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: add schema
export const handlers = [
	rest.delete<string[]>('/umbraco/backoffice/relation-type/:id', async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		umbRelationTypeData.delete([id]);

		return res(ctx.status(200));
	}),

	rest.get('/umbraco/management/api/v1/tree/relation-type/root', (req, res, ctx) => {
		const rootItems = umbRelationTypeData.getTreeRoot();
		const response = {
			total: rootItems.length,
			items: rootItems,
		};
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/relation-type/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;

		const children = umbRelationTypeData.getTreeItemChildren(parentId);

		const response = {
			total: children.length,
			items: children,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/relation-type/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		const items = umbRelationTypeData.getTreeItem(ids);
		return res(ctx.status(200), ctx.json(items));
	}),

	rest.get(umbracoPath('/relation-type/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const RelationType = umbRelationTypeData.getById(id);

		return res(ctx.status(200), ctx.json(RelationType));
	}),

	rest.post(umbracoPath('/relation-type/:id'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbRelationTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.put(umbracoPath('/relation-type/:id'), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbRelationTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),
];
