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

	rest.post<string[]>('/umbraco/management/api/v1/media/trash', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		const trashed = umbMediaData.trash(keys);

		return res(ctx.status(200), ctx.json(trashed));
	}),
];
