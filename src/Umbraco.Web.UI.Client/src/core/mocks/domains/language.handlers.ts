import { rest } from 'msw';
import { v4 as uuidv4 } from 'uuid';
import { umbLanguagesData } from '../data/languages.data';
import type { LanguageDetails } from '@umbraco-cms/models';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/language', (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLanguagesData.getAll(skipNumber, takeNumber);

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),

	rest.get('/umbraco/management/api/v1/language/:key', (req, res, ctx) => {
		const key = req.params.key as string;

		if (!key) return;

		const item = umbLanguagesData.getByKey(key);
		return res(ctx.status(200), ctx.json(item));
	}),

	rest.post<LanguageDetails>('/umbraco/management/api/v1/language', async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		data.id = umbLanguagesData.getAll().length + 1;
		data.key = uuidv4();

		const saved = umbLanguagesData.save([data]);

		return res(ctx.status(200), ctx.json(saved[0]));
	}),

	rest.put<LanguageDetails>('/umbraco/management/api/v1/language/:key', async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		const saved = umbLanguagesData.save([data]);

		return res(ctx.status(200), ctx.json(saved[0]));
	}),

	rest.delete<LanguageDetails>('/umbraco/management/api/v1/language', async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		const deleted = umbLanguagesData.delete(data);

		return res(ctx.status(200), ctx.json(deleted));
	}),
];
