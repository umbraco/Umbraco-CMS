import { rest } from 'msw';
import { umbEntityData } from '../data/entity.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/entities/documents', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('document', req, res, ctx);
	}),

	rest.get('/umbraco/backoffice/entities/media', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('media', req, res, ctx);
	}),

	rest.get('/umbraco/backoffice/entities/members', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('member', req, res, ctx);
	}),

	rest.get('/umbraco/backoffice/entities/member-groups', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('memberGroup', req, res, ctx);
	}),

	rest.get('/umbraco/backoffice/entities/data-types', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('dataType', req, res, ctx);
	}),

	rest.get('/umbraco/backoffice/entities/document-types', (req, res, ctx) => {
		console.warn('Please move to schema');
		return handleRequest('documentType', req, res, ctx);
	}),
];

const handleRequest = (entityType: string, req: any, res: any, ctx: any) => {
	const parentKey = req.url.searchParams.get('parentKey') ?? '';
	const entities = umbEntityData.getItems(entityType, parentKey);
	return res(ctx.status(200), ctx.json(entities));
};
