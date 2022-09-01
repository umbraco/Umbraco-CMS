import { rest } from 'msw';

import { DocumentTypeEntity, umbDocumentTypeData } from '../data/document-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/document-type/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const document = umbDocumentTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<DocumentTypeEntity[]>('/umbraco/backoffice/document-type/save', (req, res, ctx) => {
		const data = req.body;
		if (!data) return;

		const saved = umbDocumentTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),
];
