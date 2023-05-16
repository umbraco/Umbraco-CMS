const { rest } = window.MockServiceWorker;
import { umbMediaData } from '../data/media.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/media/details/:id', (req, res, ctx) => {
		console.warn('Please move to schema');
		const id = req.params.id as string;
		if (!id) return;

		const media = umbMediaData.getById(id);

		return res(ctx.status(200), ctx.json([media]));
	}),

	rest.post('/umbraco/management/api/v1/media/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbMediaData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post('/umbraco/management/api/v1/media/move', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;
		umbMediaData.move(data.ids, data.destination);
		return res(ctx.status(200));
	}),

	rest.post<string[]>('/umbraco/management/api/v1/media/trash', async (req, res, ctx) => {
		const ids = await req.json();
		const trashed = umbMediaData.trash(ids);
		return res(ctx.status(200), ctx.json(trashed));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/root', (req, res, ctx) => {
		const response = umbMediaData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/children', (req, res, ctx) => {
		const parentId = req.url.searchParams.get('parentId');
		if (!parentId) return;
		const response = umbMediaData.getTreeItemChildren(parentId);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/item', (req, res, ctx) => {
		const ids = req.url.searchParams.getAll('id');
		if (!ids) return;

		const items = umbMediaData.getTreeItem(ids);

		return res(ctx.status(200), ctx.json(items));
	}),
];
