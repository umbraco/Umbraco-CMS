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

	rest.get('/umbraco/backoffice/trees/member-groups/:id', (req, res, ctx) => {
		console.warn('Please move to schema');
		const id = req.params.id as string;
		if (!id) return;

		//const int = parseInt(id);
		//const document = umbNodeData.getById(int);

		const items = [
			{
				id: '1',
				key: 'fcb3f468-97de-469d-8090-d14d068ad968',
				name: 'Group 1',
				hasChildren: false,
			},
			{
				id: '2',
				key: 'd99bfca4-551d-427d-a842-d47b756b8977',
				name: 'Group 2',
				hasChildren: false,
			},
			{
				id: '3',
				key: 'b7b7ec7c-1c4a-4a78-9a6a-0652e891972a',
				name: 'Group 3',
				hasChildren: false,
			},
		];

		return res(ctx.status(200), ctx.json(items));
	}),
];
