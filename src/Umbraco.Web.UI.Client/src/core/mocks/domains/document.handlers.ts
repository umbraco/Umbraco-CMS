import { rest } from 'msw';
import { umbDocumentData } from '../data/document.data';
import type { DocumentDetails } from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/document/details/:key', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.params.key as string;
		if (!key) return;

		const document = umbDocumentData.getByKey(key);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<DocumentDetails[]>('/umbraco/management/api/v1/document/save', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const data = await req.json();
		if (!data) return;

		const saved = umbDocumentData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<string[]>('/umbraco/management/api/v1/document/trash', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		const trashed = umbDocumentData.trash(keys);

		return res(ctx.status(200), ctx.json(trashed));
	}),
];
