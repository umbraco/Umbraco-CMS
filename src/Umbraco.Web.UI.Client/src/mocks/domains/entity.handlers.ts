import { rest } from 'msw';
import { umbEntityData } from '../data/entity.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/entities/members/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/entities/member-groups/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/entities/data-types/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/entities/document-types/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),

	rest.get('/umbraco/backoffice/entities/node/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const entities = umbEntityData.getChildren(key);
		return res(ctx.status(200), ctx.json(entities));
	}),
];
