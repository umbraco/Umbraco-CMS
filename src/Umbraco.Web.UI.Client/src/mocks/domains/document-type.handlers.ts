import { rest } from 'msw';

import { DocumentTypeEntity, umbDocumentTypeData } from '../data/document-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/document-type/:id', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const int = parseInt(id);
		const document = umbDocumentTypeData.getById(int);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<DocumentTypeEntity[]>('/umbraco/backoffice/document-type/save', (req, res, ctx) => {
		const data = req.body;
		if (!data) return;

		umbDocumentTypeData.save(data);

		return res(ctx.status(200), ctx.json(data));
	}),
];
