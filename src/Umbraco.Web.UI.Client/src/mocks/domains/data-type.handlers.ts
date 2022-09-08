import { rest } from 'msw';

import { DataTypeEntity, umbDataTypeData } from '../data/data-type.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/data-type/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([dataType]));
	}),

	rest.get('/umbraco/backoffice/data-type/by-key/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([dataType]));
	}),

	rest.post<DataTypeEntity[]>('/umbraco/backoffice/data-type/save', (req, res, ctx) => {
		const data = req.body;
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<DataTypeEntity[]>('/umbraco/backoffice/data-type/trash', (req, res, ctx) => {
		console.warn('Please move to schema');
		const key = req.body as string;

		const trashed = umbDataTypeData.trash(key);

		return res(ctx.status(200), ctx.json([trashed]));
	}),
];
