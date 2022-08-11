import { rest } from 'msw';

import { NodeEntity, umbNodeData } from '../data/node.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/content/:id', (req, res, ctx) => {
		console.warn('Please move to schema');
		const id = req.params.id as string;
		if (!id) return;

		const int = parseInt(id);
		const document = umbNodeData.getById(int);

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
