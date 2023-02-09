import { rest } from 'msw';
import { umbLogviewerData } from '../data/log-viewer.data';
import { umbracoPath } from '@umbraco-cms/utils';
import { SavedLogSearch } from '@umbraco-cms/backend-api';

// TODO: add schema
export const handlers = [
	//#region Searches
	rest.get(umbracoPath('/log-viewer/saved-search'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogviewerData.searches.getSavedSearches(skipNumber, takeNumber);

		const response = {
			total: items.length,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),

	rest.get(umbracoPath('/log-viewer/saved-search/:name'), (req, res, ctx) => {
		const name = req.params.key as string;

		if (!name) return;

		const item = umbLogviewerData.searches.getByName(name);
		return res(ctx.delay(), ctx.status(200), ctx.json(item));
	}),

	rest.post<SavedLogSearch>(umbracoPath('/log-viewer/saved-search'), async (req, res, ctx) => {
		return res(ctx.delay(), ctx.status(200));
	}),

	rest.delete(umbracoPath('/log-viewer/saved-search/:name'), async (req, res, ctx) => {
		return res(ctx.status(200));
	}),
	//#endregion

	//#region Temaplates
	rest.get(umbracoPath('/log-viewer/message-template'), (req, res, ctx) => {
		const skip = req.url.searchParams.get('skip');
		const skipNumber = skip ? Number.parseInt(skip) : undefined;
		const take = req.url.searchParams.get('take');
		const takeNumber = take ? Number.parseInt(take) : undefined;

		const items = umbLogviewerData.templates.getTemplates(skipNumber, takeNumber);

		const response = {
			total: umbLogviewerData.templates.total,
			items,
		};

		return res(ctx.delay(), ctx.status(200), ctx.json(response));
	}),
	//#endregion
];
