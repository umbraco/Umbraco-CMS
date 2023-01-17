import { rest } from 'msw';
import { umbLanguagesData } from '../data/languages.data';

// TODO: add schema
export const handlers = [
	rest.get('/umbraco/management/api/v1/language', (req, res, ctx) => {
		const skip = Number.parseInt(req.url.searchParams.get('skip') ?? '0');
		const take = Number.parseInt(req.url.searchParams.get('take') ?? '100');

		const items = umbLanguagesData.getAll(skip, take);

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.status(200), ctx.json(response));
	}),
];
