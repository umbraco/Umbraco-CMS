const { rest } = window.MockServiceWorker;
import { umbLanguagesData } from '../data/languages.data.js';
import { LanguageResponseModel, ProblemDetailsModel } from '@umbraco-cms/backoffice/backend-api';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

// TODO: add schema
export const handlers = [
	rest.get(umbracoPath('/language/item'), (req, res, ctx) => {
		const isoCodes = req.url.searchParams.getAll('isoCode');
		if (!isoCodes) return;
		const items = umbLanguagesData.getItems(isoCodes);
		return res(ctx.status(200), ctx.json(items));
	}),

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

	rest.get(umbracoPath('/language/:id'), (req, res, ctx) => {
		const id = req.params.id as string;

		if (!id) return;

		const item = umbLanguagesData.getByKey(id);
		return res(ctx.status(200), ctx.json(item));
	}),

	rest.post<LanguageResponseModel>(umbracoPath('/language'), async (req, res, ctx) => {
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

	rest.put<LanguageResponseModel>(umbracoPath('/language/:id'), async (req, res, ctx) => {
		const data = await req.json();

		if (!data) return;

		umbLanguagesData.save([data]);

		return res(ctx.status(200));
	}),

	rest.delete(umbracoPath('/language/:id'), async (req, res, ctx) => {
		return res(ctx.status(200));
	}),
];
