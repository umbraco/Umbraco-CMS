import { rest } from 'msw';

import { NodeEntity, umbNodeData } from '../data/node.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/trees/members/:id', (req, res, ctx) => {
		console.warn('Please move to schema');
		const id = req.params.id as string;
		if (!id) return;

		//const int = parseInt(id);
		//const document = umbNodeData.getById(int);

		const items = [
			{
				id: '1',
				key: '865a11f9-d140-4f21-8dfe-2caafc65a971',
				name: 'John Doe',
				hasChildren: false,
			},
		];

		return res(ctx.status(200), ctx.json(items));
	}),
];
