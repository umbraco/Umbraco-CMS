import { rest } from 'msw';

import { DataTypeEntity, umbDataTypeData } from '../data/data-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/data-type/:id', (req, res, ctx) => {
		const id = req.params.id as string;
		if (!id) return;

		const int = parseInt(id);
		const document = umbDataTypeData.getById(int);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.get('/umbraco/backoffice/data-type/by-key/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const document = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([document]));
	}),

	rest.post<DataTypeEntity[]>('/umbraco/backoffice/data-type/save', (req, res, ctx) => {
		const data = req.body;
		if (!data) return;

		umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(data));
	}),
];
