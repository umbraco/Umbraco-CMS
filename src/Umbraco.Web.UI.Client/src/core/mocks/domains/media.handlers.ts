import { rest } from 'msw';
import { umbMediaData } from '../data/media.data';
import type { MediaDetails } from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/media/details/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const media = umbMediaData.getByKey(key);

		return res(ctx.status(200), ctx.json([media]));
	}),

	rest.post<MediaDetails[]>('/umbraco/management/api/v1/media/save', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const data = await req.json();
		if (!data) return;

		const saved = umbMediaData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<MediaDetails[]>('/umbraco/management/api/v1/media/move', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const data = await req.json();
		if (!data) return;

		const moved = umbMediaData.move(data.keys, data.destination);

		return res(ctx.status(200), ctx.json(moved));
	}),

	rest.post<string[]>('/umbraco/management/api/v1/media/trash', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		const trashed = umbMediaData.trash(keys);

		return res(ctx.status(200), ctx.json(trashed));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/root', (req, res, ctx) => {
		const response = umbMediaData.getTreeRoot();
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/children', (req, res, ctx) => {
		const parentKey = req.url.searchParams.get('parentKey');
		if (!parentKey) return;
		const response = umbMediaData.getTreeItemChildren(parentKey);
		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/tree/media/item', (req, res, ctx) => {
		const keys = req.url.searchParams.getAll('key');
		if (!keys) return;

		const items = umbMediaData.getTreeItem(keys);

		return res(ctx.status(200), ctx.json(items));
	}),
];
