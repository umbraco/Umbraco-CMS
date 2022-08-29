import { rest } from 'msw';
import { umbEntityData } from '../data/entity.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/trees/members/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/trees/member-groups/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/trees/data-types/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/trees/document-types/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),
];
