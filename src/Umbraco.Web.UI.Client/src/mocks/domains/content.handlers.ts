import { rest } from 'msw';

import { NodeEntity, umbNodeData } from '../data/node.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/content/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const document = umbNodeData.getByKey(key);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<NodeEntity[]>('/umbraco/backoffice/content/save', (req, res, ctx) => {
		console.warn('Please move to schema');
		const data = req.body;
		if (!data) return;

		umbNodeData.save(data);

		return res(ctx.status(200), ctx.json(data));
	}),
];
