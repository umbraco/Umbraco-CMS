const { rest } = window.MockServiceWorker;
import { umbDocumentData } from '../data/document.data.js';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: add schema
export const handlers = [
	rest.post<string[]>('/umbraco/management/api/v1/document/trash', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const ids = await req.json();

		const trashed = umbDocumentData.trash(ids);

		return res(ctx.status(200), ctx.json(trashed));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/root', (req, res, ctx) => {
		const response = umbDocumentData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const response = umbDocumentData.getTreeItemChildren(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/document/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;
		debugger;
		const items = umbDocumentData.getTreeItem(ids);

		return res(ctx.status(200), ctx.json(items));
	}),

	rest.post(umbracoPath(`/document`), async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		umbDocumentData.insert(data);

		return res(ctx.status(200));
	}),

	rest.put(umbracoPath(`/document/:id`), async (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;
		const data = await req.json();
		if (!data) return;

		const saved = umbDocumentData.save(id, data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.get(umbracoPath('/document/:id'), (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const document = umbDocumentData.getById(id);

		return res(ctx.status(200), ctx.json(document));
	}),
];
