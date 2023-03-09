import { rest } from 'msw';
import { umbLanguagesData } from '../data/languages.data';
import { LanguageModel, ProblemDetailsModel } from '@umbraco-cms/backend-api';
import { umbracoPath } from '@umbraco-cms/utils';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/language'), (req, res, ctx) => {
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

	rest.get(umbracoPath('/language/:key'), (req, res, ctx) => {
		const key = req.params.key as string;

		if (!key) return;

		const item = umbLanguagesData.getByKey(key);
		return res(ctx.status(200), ctx.json(item));
	}),

	rest.post<LanguageModel>(umbracoPath('/language'), async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		try {
			umbLanguagesData.insert(data);
			return res(ctx.status(201));
		} catch (error) {
			return res(
				ctx.status(400),
				ctx.json<ProblemDetailsModel>({
					status: 400,
					type: 'validation',
					detail: 'Something went wrong',
					errors: {
						isoCode: ['Language with same iso code already exists'],
					},
				})
			);
		}
	}),

	rest.put<LanguageModel>(umbracoPath('/language/:key'), async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		umbLanguagesData.save([data]);

		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/language/:key'), async (req, res, ctx) => {
		return res(ctx.status(200));
	}),
];
