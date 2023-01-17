import { rest } from 'msw';
import { umbLanguagesData } from '../data/languages.data';

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
];
