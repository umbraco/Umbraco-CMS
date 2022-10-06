import { rest } from 'msw';

import { NodeEntity, umbNodeData } from '../data/node.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/node/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const document = umbNodeData.getByKey(key);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<NodeEntity[]>('/umbraco/backoffice/node/save', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const data = await req.json();
		if (!data) return;

		const saved = umbNodeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<string[]>('/umbraco/backoffice/node/trash', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		const trashed = umbNodeData.trash(keys);

		return res(ctx.status(200), ctx.json(trashed));
	}),
];
