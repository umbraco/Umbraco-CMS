import { rest } from 'msw';
import { umbEntityData } from '../data/entity.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/entities', (req, res, ctx) => {
		console.warn('Please move to schema');
		const entityType = req.url.searchParams.get('type') ?? '';
		const parentKey = req.url.searchParams.get('parentKey') ?? '';
		const entities = umbEntityData.getItems(entityType, parentKey);
		return res(ctx.status(200), ctx.json(entities));
	}),
];
