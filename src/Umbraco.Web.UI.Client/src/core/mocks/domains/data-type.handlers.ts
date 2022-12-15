import { rest } from 'msw';
import { umbDataTypeData } from '../data/data-type.data';
import type { DataTypeDetails } from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/backoffice/data-type/details/:key', (req, res, ctx) => {
		const key = req.params.key as string;
		if (!key) return;

		const dataType = umbDataTypeData.getByKey(key);

		return res(ctx.status(200), ctx.json([dataType]));
	}),

	rest.post<DataTypeDetails[]>('/umbraco/backoffice/data-type/save', async (req, res, ctx) => {
		const data = await req.json();
		if (!data) return;

		const saved = umbDataTypeData.save(data);

		return res(ctx.status(200), ctx.json(saved));
	}),

	rest.post<string[]>('/umbraco/backoffice/data-type/delete', async (req, res, ctx) => {
		console.warn('Please move to schema');
		const keys = await req.json();

		umbDataTypeData.delete(keys);

		return res(ctx.status(200));
	}),
];
